// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Spellbound.Core.ECS;
using Spellbound.Core.Logging;
using Spellbound.Core.Packing;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Jobs;

namespace Spellbound.Core {
    /// <summary>
    /// Poco1
    /// Poco to assist management of object data by the parent.
    /// </summary>
    public class ObjectParent : IDisposable {
        private readonly IObjectParent _implementer;
        private readonly Transform _transform;
        public Transform Transform => _transform;

        public Vector3 LastPovPosition;
        
        public IObjectDataAccess DataAccess { get; }

        public readonly Dictionary<int, IEventSurface> EventSurfaces = new();
        public readonly Dictionary<int, Entity> Entities = new();

        public void Dispose() {
            DataAccess.OnInstanceRemoved -= HandleInstanceRemoved;
            DataAccess.OnInstanceCreated -= HandleInstanceAdded;

            var world = World.DefaultGameObjectInjectionWorld;
            if (world is not { IsCreated: true })
                return;

            var em = world.EntityManager;
            using var entities = new NativeArray<Entity>(Entities.Values.ToArray(), Allocator.Temp);
            em.DestroyEntity(entities);
        }

        public ObjectParent(
            IObjectParent implementer, Transform transform, IObjectDataAccess dataAccess, Vector3Int parentId) {
            _implementer = implementer;
            _transform = transform;
            DataAccess = dataAccess;
            DataAccess.OnInstanceRemoved += HandleInstanceRemoved;
            DataAccess.OnInstanceCreated += HandleInstanceAdded;
        }

        public void CreateNewInstance(ObjectPreset preset, Vector3 position, Vector3 rotation, int scale) =>
                DataAccess.CreateInstance(preset.presetUid, position, rotation, scale);

        public void CreateNewInstanceWithData<T>(ObjectPreset preset, Vector3 position, Vector3 rotation, int scale,
            int eventSurfaceIndex, T data) where T : IPacker, new() =>
                DataAccess.CreateInstanceWithData(preset.presetUid, position, rotation, scale, eventSurfaceIndex, data);
        public void ActivateObjects(NativeList<ProceduralObjectData> objects) {
            if (!objects.IsCreated)
                return;
            
            DataAccess.SetProceduralInstanceCount(objects.Length);
            
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;

            for (var i = 0; i < objects.Length; i++) {
                if (DataAccess.IsDeleted(i)) {
                    continue;
                }

                var entity = em.Instantiate(objects[i].entityPrefab);

                em.SetComponentData(entity, LocalTransform.FromPositionRotationScale(
                    objects[i].position,
                    quaternion.Euler(objects[i].rotation),
                    objects[i].scale.x
                ));

                em.SetComponentData(entity, new InstanceIndexComponent {
                    Value = i
                });
                
                Entities[i] = entity;

            }
            
            foreach (var kvp in DataAccess.GetAllInstances()) {
                if (kvp.Value.Transform == null) {
                    continue;
                }

                if (kvp.Key >= objects.Length) {
                    HandleInstanceAdded(kvp.Key, kvp.Value.PresetUid, kvp.Value.Transform.Value);
                }
            }
        }

        private void HandleInstanceAdded(
            int instanceIndex, string presetUid, TransformData transformData) {
            HandleInstanceAdded(instanceIndex, 
                presetUid, 
                transformData.Position, 
                quaternion.Euler(transformData.Rotation), 
                transformData.Scale);
            
        }
        
        public void HandleInstanceAdded(
            int instanceIndex, string presetUid, Vector3 position, Quaternion rotation, float scale) {
            if (!presetUid.TryGetEntityPrefab(out var prefab)) return;
            
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;

            var entity = em.Instantiate(prefab);

            em.SetComponentData(entity, LocalTransform.FromPositionRotationScale(
                position,
                rotation,
                scale
            ));

            em.SetComponentData(entity, new InstanceIndexComponent {
                Value = instanceIndex
            });
            Entities[instanceIndex] = entity;
            
        }

        public bool TryReadData<T>(int instanceIndex, string presetUid, int eventSurfaceIndex, out T result)
                where T : IPacker, new() {
            if (DataAccess.TryRead<T>(instanceIndex, eventSurfaceIndex, out var data)) {
                result = data;
                Debug.Log($"result is {result}");

                return true;
            }

            result = default;

            return false;
        }

        public bool TryWriteData<T>(int instanceIndex, string presetUid, int eventSurfaceIndex, T newData)
                where T : IPacker, new() {
            DataAccess.Write(instanceIndex, presetUid, eventSurfaceIndex, newData);

            return true;
        }

        public bool TryTransformData<T>(
            int instanceIndex, string presetUid, int eventSurfaceIndex, T delta) where T : IQuantitativeData, new() {
            Debug.Log($"Calling TryTransformData on instanceIndex {instanceIndex}, delta {delta}");

            DataAccess.Delta(instanceIndex, presetUid, eventSurfaceIndex, delta);

            return true;
        }

        public async Task<bool> TryDeleteData(int instanceIndex) => await DataAccess.TryDeleteInstance(instanceIndex);

        private void HandleInstanceRemoved(int instanceIndex) {
            DeleteEntity(instanceIndex);
            DeleteEventSurface(instanceIndex);
        }

        public int Migrate(int instanceIndex, Vector3Int destinationCoord, IObjectParent newParent) {
            return DataAccess.MigrateInstance(instanceIndex, destinationCoord, newParent);
        }

        private void DeleteEventSurface(int instanceIndex) {
            if (!EventSurfaces.TryGetValue(instanceIndex, out var surface)) return;

            UnityEngine.Object.Destroy(surface.GetGameObject());
            EventSurfaces.Remove(instanceIndex);
        }

        public void DeleteEntity(int instanceIndex) {
            if (!Entities.TryGetValue(instanceIndex, out var entity)) return;

            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            em.DestroyEntity(entity);
            Entities.Remove(instanceIndex);
        }

        public void SpawnSurface(int instanceIndex) {
            
            if (!Entities.TryGetValue(instanceIndex, out var entity)) {
                Log.Error($"entity is not in the dictionary");
                return;
            }
            
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            
            if (!em.Exists(entity)) {
                Log.Error($"entity is null");
                return;
            }
                
            var localTransform = em.GetComponentData<LocalTransform>(entity);
            var presetUid = em.GetComponentData<PresetUidComponent>(entity).Value;
            var preset = presetUid.Value.ResolvePreset();
            
            var eventSurfaceObj = UnityEngine.Object.Instantiate(
                preset.eventSurfacePrefab,
                localTransform.Position,
                localTransform.Rotation,
                _transform
            );
            eventSurfaceObj.gameObject.name = $"{preset.name} Event Surface {instanceIndex}";
            eventSurfaceObj.transform.localScale = localTransform.Scale * Vector3.one;
            
            var eventSurface = eventSurfaceObj.GetComponent<IEventSurface>();
            eventSurface.Initialize(_implementer, instanceIndex, preset.presetUid);

            if (eventSurface is StaticEventSurface staticEventSurface) {
                EventSurfaces[instanceIndex] = eventSurface;

                return;
            }
            DeleteEntity(instanceIndex);
            
            
        }

        public void FlagForDestroySurface(int instanceIndex) {
            if (!EventSurfaces.TryGetValue(instanceIndex, out var eventSurface))
                return;

            eventSurface.FlagForDestroy();
        }

        public void EntityDistanceQuery(Vector3 playerPosition) {
            Log.Debug("Entity distance query");
            LastPovPosition = playerPosition;
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            var existingEntities = new NativeList<ProximityEntity>(Entities.Count, Allocator.TempJob);

            foreach (var kvp in Entities) {

                if (!em.Exists(kvp.Value)) {
                    continue;
                }
                
                var localTransform = em.GetComponentData<LocalTransform>(kvp.Value);
                var presetUid = em.GetComponentData<PresetUidComponent>(kvp.Value).Value;
                var isDynamic = em.HasComponent<DynamicTag>(kvp.Value);
                var preset = presetUid.Value.ResolvePreset();
                
                var pos = new int3(
                    (int)math.round(localTransform.Position.x),
                    (int)math.round(localTransform.Position.y),
                    (int)math.round(localTransform.Position.z)
                );
                existingEntities.Add(new ProximityEntity() {
                    Position = pos,
                    Thresholds = preset.interactionDistance,
                    InstanceIndex = kvp.Key,
                    IsDynamic = isDynamic
                });
            }
            
            var povs = new NativeArray<int3>(1, Allocator.TempJob);
            povs[0] = new int3((int)math.round(playerPosition.x), 
                (int)math.round(playerPosition.y), 
                (int)math.round(playerPosition.z)
                );
            
            var existingEventSurfaces = new NativeHashSet<int>(EventSurfaces.Count, Allocator.TempJob);
            foreach (var key in EventSurfaces.Keys)
                existingEventSurfaces.Add(key);
            var instancesToAwaken = new NativeParallelHashSet<int>(existingEntities.Length, Allocator.TempJob);
            var instancesToSleep = new NativeParallelHashSet<int>(existingEntities.Length, Allocator.TempJob);
            
            var eventSurfaceJob = new ProximityEventSurfaceJob() {
                PovPositions = povs,
                ProximityEntities = existingEntities,
                ExistingEventSurfaces = existingEventSurfaces,
                InstancesToAwaken = instancesToAwaken.AsParallelWriter(),
                InstancesToSleep = instancesToSleep.AsParallelWriter()
            };
            var eventSurfaceJobHandle = eventSurfaceJob.Schedule(existingEntities.Length, 64);
            eventSurfaceJobHandle.Complete();
            
            Log.Debug($"Entity distance query Ran the job with awaken {instancesToAwaken.Count()} and sleep {instancesToSleep.Count()} ");

            foreach (var instanceToAwake in instancesToAwaken) {
                SpawnSurface(instanceToAwake);
            }
            
            foreach (var instanceToSleep in instancesToSleep) {
                FlagForDestroySurface(instanceToSleep);
            }
            
            existingEntities.Dispose();
            povs.Dispose();
            existingEventSurfaces.Dispose();
            instancesToAwaken.Dispose();
            instancesToSleep.Dispose();
        }
    }
}