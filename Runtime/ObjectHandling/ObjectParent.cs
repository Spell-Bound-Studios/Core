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

        public readonly Dictionary<int, IEventSurface> StaticEventSurfaceDict = new();
        public readonly Dictionary<int, Entity> Entities = new();

        public void Dispose() {
            DataAccess.OnInstanceRemoved -= HandleInstanceRemoved;
            //DataAccess.OnInstanceCreated -= CreateEntity;

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
            // DataAccess.OnInstanceCreated += CreateEntity;
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
                if (DataAccess.IsDeleted(i))
                    continue;

                var entity = em.Instantiate(objects[i].entityPrefab);

                em.SetComponentData(entity, LocalTransform.FromPositionRotationScale(
                    objects[i].position,
                    quaternion.Euler(math.radians(objects[i].rotation)),
                    objects[i].scale.x
                ));

                em.SetComponentData(entity, new InstanceIndexComponent {
                    Value = i
                });
        
                Entities[i] = entity;
            }
    
            foreach (var kvp in DataAccess.GetAllInstances()) {
                if (kvp.Value.Transform == null)
                    continue;

                if (kvp.Key >= objects.Length) {
                    if (kvp.Value.PresetUid.ResolvePreset().isDynamic) {
                        DataAccess.CreateDynamicInstanceFunc?.Invoke(kvp.Key, kvp.Value.PresetUid, kvp.Value.Transform.Value);

                        continue;
                    }
                    DataAccess.CreateStaticInstanceFunc?.Invoke(kvp.Key, kvp.Value.PresetUid, kvp.Value.Transform.Value);
                }
                   
            }
        }

        public bool CreateEntity(
            int instanceIndex, string presetUid, TransformData transformData) {
            return CreateEntity(instanceIndex, 
                presetUid, 
                transformData.Position, 
                quaternion.Euler(math.radians(transformData.Rotation)), 
                transformData.Scale);
        }
        
        public bool CreateEntity(
            int instanceIndex, string presetUid, Vector3 position, Quaternion rotation, float scale) {

            if (Entities.ContainsKey(instanceIndex)) {
                Log.Error($"Trying to create an entity but it's already in the dictionary. InstanceIndex: {instanceIndex}, chunk: {_transform.name}");

                return false;
            }

            if (!presetUid.TryGetEntityPrefab(out var prefab)) {
                Log.Error($"Trying to create an entity but cant find EntityPrefab. InstanceIndex: {instanceIndex}, chunk: {_transform.name}");
                return false;
            }
            
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
            return true;
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

        public int Migrate(int instanceIndex, IEventSurface eventSurface, Vector3Int destinationCoord, IObjectParent newParent) {
            return DataAccess.MigrateInstance(instanceIndex, eventSurface, destinationCoord, newParent);
        }

        public void RefreshInstanceTransform(int instanceIndex, TransformData transformData) {
            DataAccess.RefreshInstanceTransform(instanceIndex, transformData);
        }

        private void DeleteEventSurface(int instanceIndex) {
            if (!StaticEventSurfaceDict.TryGetValue(instanceIndex, out var surface)) return;

            UnityEngine.Object.Destroy(surface.GetGameObject());
            StaticEventSurfaceDict.Remove(instanceIndex);
        }

        public void DeleteEntity(int instanceIndex) {
            if (!Entities.TryGetValue(instanceIndex, out var entity)) return;

            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            em.DestroyEntity(entity);
            Entities.Remove(instanceIndex);
            Log.Debug($"Instance {instanceIndex} removed from entities Dictionary");
        }
        
        private bool TryCreateStaticInstance(int instanceIndex, string presetUid, TransformData transformData) {
            var preset = presetUid.ResolvePreset();
            if ( math.distancesq(LastPovPosition, transformData.Position) <= preset.interactionDistance.x * preset.interactionDistance.x) {
                return SpawnStaticSurface(instanceIndex, presetUid, transformData);
            }

            return false;
        }

        public bool SpawnDynamicSurface(
            int instanceIndex, string presetUid, TransformData transformData) {
            if (DataAccess.DynamicEventSurfaceDict.ContainsKey(instanceIndex)) {
                Log.Error($"Instance {instanceIndex} already exists in DynamicEventSurfaceDict in chunk {_transform.gameObject.name}");

                return false;
            }

            var preset = presetUid.ResolvePreset();
            var eventSurfaceObj = UnityEngine.Object.Instantiate(
                preset.eventSurfacePrefab,
                transformData.Position,
                Quaternion.Euler(transformData.Rotation),
                _transform
            );
            eventSurfaceObj.gameObject.name = $"{preset.name} Event Surface {instanceIndex}";
            eventSurfaceObj.transform.localScale = transformData.Scale * Vector3.one;
            
            var eventSurface = eventSurfaceObj.GetComponent<IEventSurface>();
            eventSurface.Initialize(_implementer, instanceIndex, preset.presetUid);
            DataAccess.DynamicEventSurfaceDict[instanceIndex] = eventSurface;

            return true;
        }
        
        public void SpawnDynamicSurface(int instanceIndex) {
            
            if (!Entities.TryGetValue(instanceIndex, out var entity)) {
                Log.Error($"entity is not in the dictionary with instanceIndex {instanceIndex}");
                return;
            }
            
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            
            if (!em.Exists(entity)) {
                Log.Error($"entity is null");
                return;
            }
                
            var localTransform = em.GetComponentData<LocalTransform>(entity);
            var presetUid = em.GetComponentData<PresetUidComponent>(entity).Value;
            var transformData = new TransformData(localTransform.Position, math.degrees(math.Euler(localTransform.Rotation)), localTransform.Scale);
            
            SpawnDynamicSurface(instanceIndex, presetUid.Value, transformData);
        }

        
        public bool SpawnStaticSurface(
            int instanceIndex, string presetUid, TransformData transformData) {
            if (StaticEventSurfaceDict.ContainsKey(instanceIndex)) {
                Log.Error($"Instance {instanceIndex} already exists in StaticEventSurfaceDict in chunk {_transform.gameObject.name}");

                return false;
            }
            
            var preset = presetUid.ResolvePreset();
            var eventSurfaceObj = UnityEngine.Object.Instantiate(
                preset.eventSurfacePrefab,
                transformData.Position,
                Quaternion.Euler(transformData.Rotation),
                _transform
            );
            eventSurfaceObj.gameObject.name = $"{preset.name} Event Surface {instanceIndex}";
            eventSurfaceObj.transform.localScale = transformData.Scale * Vector3.one;
            
            var eventSurface = eventSurfaceObj.GetComponent<IEventSurface>();
            eventSurface.Initialize(_implementer, instanceIndex, preset.presetUid);
            StaticEventSurfaceDict[instanceIndex] = eventSurface;

            return true;
        }
        public void SpawnStaticSurface(int instanceIndex) {
            
            if (!Entities.TryGetValue(instanceIndex, out var entity)) {
                Log.Error($"entity is not in the dictionary with instanceIndex {instanceIndex}");
                return;
            }
            
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            
            if (!em.Exists(entity)) {
                Log.Error($"entity is null");
                return;
            }
                
            var localTransform = em.GetComponentData<LocalTransform>(entity);
            var presetUid = em.GetComponentData<PresetUidComponent>(entity).Value;
            var transformData = new TransformData(localTransform.Position, math.degrees(math.Euler(localTransform.Rotation)), localTransform.Scale);
            
            SpawnStaticSurface(instanceIndex, presetUid.Value, transformData);
        }
        

        public void DynamicEntityDistanceQuery(float3[] povPositions) {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            var dynamicEntities = new NativeList<ProximityCandidate>(Entities.Count, Allocator.TempJob);
            var activeSurfaces = new NativeList<ProximityCandidate>(DataAccess.DynamicEventSurfaceDict.Count, Allocator.TempJob);

            foreach (var kvp in Entities) {
                if (!em.Exists(kvp.Value)) continue;
                if (!em.HasComponent<DynamicTag>(kvp.Value)) continue;

                var localTransform = em.GetComponentData<LocalTransform>(kvp.Value);
                var presetUid = em.GetComponentData<PresetUidComponent>(kvp.Value).Value;
                var preset = presetUid.Value.ResolvePreset();


                dynamicEntities.Add(new ProximityCandidate() {
                    Position = localTransform.Position,
                    Thresholds = preset.interactionDistance,
                    InstanceIndex = kvp.Key,
                });
            }

            foreach (var dynamicEventSurface in DataAccess.DynamicEventSurfaceDict.Values) {
                activeSurfaces.Add(dynamicEventSurface.ProximityCandidate);
            }
            
            var povs = new NativeArray<float3>(povPositions, Allocator.TempJob);
            
            var instancesToAwaken = new NativeParallelHashSet<int>(dynamicEntities.Length, Allocator.TempJob);
            var instancesToSleep = new NativeParallelHashSet<int>(activeSurfaces.Length, Allocator.TempJob);

            var awakenJob = new DynamicEventSurfaceAwakenJob() {
                PovPositions = povs,
                ProximityEntities = dynamicEntities,
                InstancesToAwaken = instancesToAwaken.AsParallelWriter()
            };

            var sleepJob = new DynamicEventSurfaceSleepJob() {
                PovPositions = povs,
                ProximityEntities = activeSurfaces,
                InstancesToSleep = instancesToSleep.AsParallelWriter()
            };

            var awakenHandle = awakenJob.Schedule(dynamicEntities.Length, 64);
            var sleepHandle = sleepJob.Schedule(activeSurfaces.Length, 64);

            JobHandle.CombineDependencies(awakenHandle, sleepHandle).Complete();

            foreach (var instanceIndex in instancesToAwaken) {
                if (DataAccess.DynamicEventSurfaceDict.TryGetValue(instanceIndex, out var eventSurface)) {
                    // should never happen?
                    eventSurface.FlagForDestroy(false);
                    continue;
                }
                SpawnDynamicSurface(instanceIndex);
                DeleteEntity(instanceIndex);
            }
                

            foreach (var instanceIndex in instancesToSleep) {
                if (DataAccess.DynamicEventSurfaceDict.TryGetValue(instanceIndex, out var eventSurface)) {

                    eventSurface.FlagForDestroy(true);
                }
                
            }
                

            dynamicEntities.Dispose();
            activeSurfaces.Dispose();
            povs.Dispose();
            instancesToAwaken.Dispose();
            instancesToSleep.Dispose();
        }


        public void StaticEntityDistanceQuery(float3[] povPositions) {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            var existingEntities = new NativeList<ProximityCandidate>(Entities.Count, Allocator.TempJob);

            foreach (var kvp in Entities) {

                if (!em.Exists(kvp.Value)) {
                    continue;
                }

                if (em.HasComponent<DynamicTag>(kvp.Value)) {
                    continue;
                }
                
                var localTransform = em.GetComponentData<LocalTransform>(kvp.Value);
                var presetUid = em.GetComponentData<PresetUidComponent>(kvp.Value).Value;
                var preset = presetUid.Value.ResolvePreset();
                
                existingEntities.Add(new ProximityCandidate() {
                    Position = localTransform.Position,
                    Thresholds = preset.interactionDistance,
                    InstanceIndex = kvp.Key,
                });
            }
            
            var povs = new NativeArray<float3>(povPositions, Allocator.TempJob);

            
            var existingEventSurfaces = new NativeHashSet<int>(StaticEventSurfaceDict.Count, Allocator.TempJob);
            foreach (var key in StaticEventSurfaceDict.Keys)
                existingEventSurfaces.Add(key);
            var instancesToAwaken = new NativeParallelHashSet<int>(existingEntities.Length, Allocator.TempJob);
            var instancesToSleep = new NativeParallelHashSet<int>(existingEntities.Length, Allocator.TempJob);
            
            var eventSurfaceJob = new StaticEventSurfaceProximityJob() {
                PovPositions = povs,
                ProximityEntities = existingEntities,
                ExistingEventSurfaces = existingEventSurfaces,
                InstancesToAwaken = instancesToAwaken.AsParallelWriter(),
                InstancesToSleep = instancesToSleep.AsParallelWriter()
            };
            var eventSurfaceJobHandle = eventSurfaceJob.Schedule(existingEntities.Length, 64);
            eventSurfaceJobHandle.Complete();

            foreach (var instanceIndex in instancesToAwaken) {
                if (StaticEventSurfaceDict.TryGetValue(instanceIndex, out var surface)) {
                    surface.FlagForDestroy(false); 
                } else {
                    SpawnStaticSurface(instanceIndex);
                }
            }
            
            foreach (var instanceToSleep in instancesToSleep) {
                if (StaticEventSurfaceDict.TryGetValue(instanceToSleep, out var surface)) {
                    surface.FlagForDestroy(true);
                }
            }
            
            existingEntities.Dispose();
            povs.Dispose();
            existingEventSurfaces.Dispose();
            instancesToAwaken.Dispose();
            instancesToSleep.Dispose();
        }
    }
}