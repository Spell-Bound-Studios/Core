// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Spellbound.Core.ECS;
using Spellbound.Core.Logging;
using Spellbound.Core.Packing;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Spellbound.Core {
    /// <summary>
    /// Poco1
    /// Poco to assist management of object data by the parent.
    /// </summary>
    public class ObjectParent : IDisposable {
        private readonly IObjectParent _implementer;
        private readonly Transform _transform;
        private int3 _id;
        private EntityQuery _query;
        private Entity _ecsChunk;

        private Action<LocalTransform, int, ObjectPreset> _surfaceSpawnAction;
        public IObjectDataAccess DataAccess { get; }

        private readonly Dictionary<int, EventSurface> _eventSurfaces = new();

        public ObjectParent(
            IObjectParent implementer, Transform transform, IObjectDataAccess dataAccess, Vector3Int parentId,
            Entity ecsChunk) {
            _implementer = implementer;
            _transform = transform;
            _id = new int3(parentId.x, parentId.y, parentId.z);
            _ecsChunk = ecsChunk;

            var em = World.DefaultGameObjectInjectionWorld.EntityManager;

            _query = em.CreateEntityQuery(
                ComponentType.ReadOnly<ChunkParentComponent>(),
                ComponentType.ReadOnly<LocalTransform>(),
                ComponentType.ReadOnly<StaticProximityObjectComponent>(),
                ComponentType.ReadOnly<InstanceIndexComponent>(),
                ComponentType.ReadOnly<PresetUidComponent>()
            );
            _query.SetSharedComponentFilter(new ChunkParentComponent { ChunkCoord = _id });

            DataAccess = dataAccess;
            DataAccess.OnInstanceRemoved += HandleInstanceRemoved;
            DataAccess.OnInstanceCreated += HandleInstanceAdded;
        }

        public void SetEcsChunkReadyForObjects() {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            em.AddComponentData(_ecsChunk, new ReadyForObjectsTag());
        }

        public void BufferFullStateObjects() {
            var instances = DataAccess.GetAllInstances();

            if (instances.Count == 0) return;

            var em = World.DefaultGameObjectInjectionWorld.EntityManager;

            var buffer = em.HasBuffer<EntitySpawnRequestElement>(_ecsChunk)
                    ? em.GetBuffer<EntitySpawnRequestElement>(_ecsChunk)
                    : em.AddBuffer<EntitySpawnRequestElement>(_ecsChunk);

            foreach (var instance in instances) {
                if (instance.Key < DataAccess.ProceduralInstanceIndexCount) continue;

                if (instance.Value.Transform == null) continue;

                if (!instance.Value.PresetUid.TryGetEntityPrefab(out var prefab)) {
                    Log.Error($"Entity prefab could not be found: {instance.Value.PresetUid}");

                    continue;
                }

                buffer.Add(new EntitySpawnRequestElement {
                    Prefab = prefab,
                    InstanceIndex = instance.Key,
                    Transform = LocalTransform.FromPositionRotationScale(
                        instance.Value.Transform.Value.Position,
                        quaternion.Euler(instance.Value.Transform.Value.Rotation),
                        instance.Value.Transform.Value.Scale
                    )
                });
            }
        }

        public void BufferFullStateDeletions() {
            var deletions = DataAccess.GetAllDeletions();

            if (deletions.Count == 0) return;

            var em = World.DefaultGameObjectInjectionWorld.EntityManager;

            var buffer = em.AddBuffer<DeletionBufferElement>(_ecsChunk);

            foreach (var deletion in DataAccess.GetAllDeletions())
                buffer.Add(new DeletionBufferElement { Value = deletion });
        }

        public void CreateNewInstance(ObjectPreset preset, Vector3 position, Vector3 rotation, int scale) {
            DataAccess.CreateInstance(preset.presetUid, position, rotation, scale);
        }
        
        
        public void CreateNewInstanceWithData<T>(
            ObjectPreset preset, Vector3 position, Vector3 rotation, int scale,
            int eventSurfaceIndex, T data) where T : IPacker, new() =>
                DataAccess.CreateInstanceWithData(preset.presetUid, position, rotation, scale, eventSurfaceIndex, data);

        private void HandleInstanceAdded(
            int instanceIndex, string presetUid, TransformData transformData) {
            if (!presetUid.TryGetEntityPrefab(out var prefab))
                return;

            var em = World.DefaultGameObjectInjectionWorld.EntityManager;

            var entity = em.Instantiate(prefab);

            em.SetComponentData(entity, LocalTransform.FromPositionRotationScale(
                transformData.Position,
                quaternion.Euler(transformData.Rotation),
                transformData.Scale
            ));

            em.SetComponentData(entity, new InstanceIndexComponent {
                Value = instanceIndex
            });

            em.SetSharedComponent(entity, new ChunkParentComponent { ChunkCoord = _id });
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

        private void DeleteEventSurface(int instanceIndex) {
            if (!_eventSurfaces.TryGetValue(instanceIndex, out var surface)) return;

            UnityEngine.Object.Destroy(surface.gameObject);
            _eventSurfaces.Remove(instanceIndex);
        }

        private void DeleteEntity(int instanceIndex) {
            var indices = _query.ToComponentDataArray<InstanceIndexComponent>(Allocator.Temp);
            var entities = _query.ToEntityArray(Allocator.Temp);

            var em = World.DefaultGameObjectInjectionWorld.EntityManager;

            for (var i = 0; i < indices.Length; i++) {
                if (indices[i].Value == instanceIndex) {
                    em.DestroyEntity(entities[i]);

                    break;
                }
            }

            indices.Dispose();
            entities.Dispose();
        }

        public void SpawnSurface(LocalTransform transform, int instanceIndex, ObjectPreset preset) {
            var eventSurface = UnityEngine.Object.Instantiate(
                preset.eventSurfacePrefab,
                transform.Position,
                transform.Rotation,
                _transform
            );
            eventSurface.gameObject.name = $"{preset.name} Event Surface {instanceIndex}";
            eventSurface.transform.localScale = transform.Scale * Vector3.one;
            eventSurface.Initialize(_implementer, instanceIndex, preset.presetUid);
            _eventSurfaces[instanceIndex] = eventSurface;
        }

        private void DespawnSurface(int instanceIndex) {
            if (!_eventSurfaces.TryGetValue(instanceIndex, out var proxy))
                return;

            UnityEngine.Object.Destroy(proxy.gameObject);
            _eventSurfaces.Remove(instanceIndex);
        }

        public void EntityDistanceQuery(float3[] povPositions) {
            
            var existingEventSurfaces = new NativeHashSet<int>(_eventSurfaces.Count, Allocator.TempJob);
            foreach (var key in _eventSurfaces.Keys)
                existingEventSurfaces.Add(key);

            var maxCapacity = _query.CalculateEntityCount();
            
            var instancesToAwaken = new NativeParallelHashMap<int, (FixedString64Bytes, LocalTransform)>(maxCapacity, Allocator.TempJob);
            var instancesToSleep = new NativeParallelHashSet<int>(maxCapacity, Allocator.TempJob);
            
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            
            var chunks = _query.ToArchetypeChunkArray(Allocator.TempJob);
            var sharedHandle = em.GetSharedComponentTypeHandle<PresetUidComponent>();
            var entityCount = _query.CalculateEntityCount();
            var presetUids = new NativeArray<PresetUidComponent>(entityCount, Allocator.TempJob);

            int entityIndex = 0;
            foreach (var chunk in chunks)
            {
                PresetUidComponent sharedValue = chunk.GetSharedComponent(sharedHandle);
                for (int i = 0; i < chunk.Count; i++)
                {
                    presetUids[entityIndex++] = sharedValue;
                }
            }
            chunks.Dispose();

            var job = new StaticEventSurfaceProximityJob {
                PovPositions = new NativeArray<float3>(povPositions, Allocator.TempJob),
                Transforms = _query.ToComponentDataArray<LocalTransform>(Allocator.TempJob),
                Thresholds = _query.ToComponentDataArray<StaticProximityObjectComponent>(Allocator.TempJob),
                InstanceIndices = _query.ToComponentDataArray<InstanceIndexComponent>(Allocator.TempJob),
                PresetUids = presetUids,
                ExistingEventSurfaces = existingEventSurfaces,

                InstancesToAwaken = instancesToAwaken.AsParallelWriter(),
                InstancesToSleep = instancesToSleep.AsParallelWriter()
            };
            var jobHandle = job.Schedule(maxCapacity, 64);
            jobHandle.Complete();

            foreach (var toAwaken in instancesToAwaken) {
                SpawnSurface(toAwaken.Value.Item2, toAwaken.Key, toAwaken.Value.Item1.Value.ResolvePreset());
            }

            foreach (var toSleep in instancesToSleep) {
                DespawnSurface(toSleep);
            }

            job.PovPositions.Dispose();
            job.Transforms.Dispose();
            job.Thresholds.Dispose();
            job.InstanceIndices.Dispose();
            job.PresetUids.Dispose();
            existingEventSurfaces.Dispose();
            instancesToAwaken.Dispose();
            instancesToSleep.Dispose();
            
        }

        #region API

        public void DestroyAllEntities() {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            var entities = _query.ToEntityArray(Allocator.Temp);
            em.DestroyEntity(entities);
            
            //TODO make it persistent
            
        }
        
        public void DestroyAllPlacedEntities() {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            var proceduralCount = DataAccess.ProceduralInstanceIndexCount;
            var entities = _query.ToEntityArray(Allocator.Temp);
            var instanceIndices = _query.ToComponentDataArray<InstanceIndexComponent>(Allocator.Temp);

            for (var i = 0; i < entities.Length; i++) {
                if (instanceIndices[i].Value < proceduralCount)
                    continue;
                em.DestroyEntity(entities[i]);
            }
            
            //TODO make it persistent
            
        }
        
        public void DestroyAllProcGenEntities() {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            var proceduralCount = DataAccess.ProceduralInstanceIndexCount;
            var entities = _query.ToEntityArray(Allocator.Temp);
            var instanceIndices = _query.ToComponentDataArray<InstanceIndexComponent>(Allocator.Temp);

            for (var i = 0; i < entities.Length; i++) {
                if (instanceIndices[i].Value >= proceduralCount)
                    continue;
                em.DestroyEntity(entities[i]);
            }
            
            //TODO make it persistent
            
        }

        #endregion

        #region IDisposable

        public void Dispose() {
            DataAccess.OnInstanceRemoved -= HandleInstanceRemoved;
            DataAccess.OnInstanceCreated -= HandleInstanceAdded;

            DisposeEcs();
        }
        
        /// <summary>
        /// Destroys all entities associated with this chunk, including the entity representing the chunk itself. 
        /// </summary>
        private void DisposeEcs() {
            var world = World.DefaultGameObjectInjectionWorld;

            if (world is not { IsCreated: true })
                return;

            var em = world.EntityManager;

            var disposeQuery = em.CreateEntityQuery(
                ComponentType.ReadOnly<ChunkParentComponent>());
            disposeQuery.SetSharedComponentFilter(new ChunkParentComponent { ChunkCoord = _id });

            var entities = disposeQuery.ToEntityArray(Allocator.Temp);
            em.DestroyEntity(entities);
            entities.Dispose();
        }

        #endregion IDisposable
    }
}