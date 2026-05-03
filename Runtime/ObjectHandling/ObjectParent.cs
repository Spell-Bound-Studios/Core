// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Spellbound.Core.ECS;
using Spellbound.Core.Logging;
using Spellbound.Core.Packing;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Spellbound.Core {
    /// <summary>
    /// Poco1
    /// Poco to assist management of object data by the parent.
    /// </summary>
    public class ObjectParent : IDisposable, IObjectInstanceConsumer {
        private readonly IObjectParent _implementer;
        private readonly Transform _transform;
        
        private readonly int3 _id;
        private EntityQuery _query;
        private readonly Entity _ecsChunk;

        private Action<LocalTransform, int, ObjectPreset> _surfaceSpawnAction;
        private readonly Dictionary<int, EventSurface> _eventSurfaces = new();
        
        public IObjectDataAccess DataAccess { get; }

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
            
            _query.SetSharedComponentFilter(new ChunkParentComponent {
                ChunkCoord = _id
            });

            DataAccess = dataAccess;
            DataAccess.SetConsumer(this);
        }
        
        #region API
        
        public void CreateNewInstance(ObjectPreset preset, Vector3 position, Vector3 rotation, int scale) =>
                DataAccess.CreateRuntimeInstance(preset.presetUid, position, rotation, scale);

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
        
        #endregion API

        #region ECS
        
        public void SetEcsChunkReadyForObjects() {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            em.AddComponentData(_ecsChunk, new ReadyForObjectsTag());
        }

        public void BufferFullStateObjects() {
            var instances = DataAccess.GetAllRuntimeInstances();

            if (instances.Count == 0) 
                return;

            var em = World.DefaultGameObjectInjectionWorld.EntityManager;

            var buffer = em.HasBuffer<EntitySpawnRequestElement>(_ecsChunk)
                    ? em.GetBuffer<EntitySpawnRequestElement>(_ecsChunk)
                    : em.AddBuffer<EntitySpawnRequestElement>(_ecsChunk);

            foreach (var instance in instances) {
                if (!instance.Value.PresetUid.TryGetEntityPrefab(out var prefab)) {
                    Log.Error($"Entity prefab could not be found: {instance.Value.PresetUid}");
                    continue;
                }

                buffer.Add(new EntitySpawnRequestElement {
                    Prefab = prefab,
                    InstanceIndex = instance.Key,
                    Transform = LocalTransform.FromPositionRotationScale(
                        instance.Value.Transform.Position,
                        quaternion.Euler(instance.Value.Transform.Rotation),
                        instance.Value.Transform.Scale
                    )
                });
            }
        }

        public void BufferFullStateDeletions() {
            var deletions = DataAccess.GetAllSeedInstanceDeletions();

            if (deletions.Count == 0) 
                return;

            var em = World.DefaultGameObjectInjectionWorld.EntityManager;

            var buffer = em.AddBuffer<DeletionBufferElement>(_ecsChunk);

            foreach (var deletion in deletions)
                buffer.Add(new DeletionBufferElement { Value = deletion });
        }
        
        private void DestroyEntities(IReadOnlyList<int> instanceIndices) {
            var removedSet = new HashSet<int>(instanceIndices);

            var indices = _query.ToComponentDataArray<InstanceIndexComponent>(Allocator.Temp);
            var entities = _query.ToEntityArray(Allocator.Temp);
            var entitiesToDestroy = new NativeList<Entity>(removedSet.Count, Allocator.Temp);

            for (var i = 0; i < indices.Length; i++) {
                if (removedSet.Contains(indices[i].Value))
                    entitiesToDestroy.Add(entities[i]);
            }

            if (entitiesToDestroy.Length > 0) {
                var em = World.DefaultGameObjectInjectionWorld.EntityManager;
                em.DestroyEntity(entitiesToDestroy.AsArray());
            }

            entitiesToDestroy.Dispose();
            indices.Dispose();
            entities.Dispose();
        }
        
        public void DestroyAllEntities() {
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
        
        #endregion ECS

        #region IObjectInstanceConsumer Implementation
        
        public void OnRuntimeInstancesCreated(IReadOnlyList<RuntimeInstanceCreation> creations) {
            if (creations.Count == 0)
                return;

            var em = World.DefaultGameObjectInjectionWorld.EntityManager;

            // Group by preset uid. One Instantiate call per group.
            var byPreset = new Dictionary<string, List<RuntimeInstanceCreation>>();

            foreach (var creation in creations) {
                if (!byPreset.TryGetValue(creation.PresetUid, out var list)) {
                    list = new List<RuntimeInstanceCreation>();
                    byPreset[creation.PresetUid] = list;
                }

                list.Add(creation);
            }

            foreach (var (presetUid, list) in byPreset) {
                if (!presetUid.TryGetEntityPrefab(out var prefab)) {
                    Log.Error($"Entity prefab could not be found: {presetUid}");
                    continue;
                }

                var entities = new NativeArray<Entity>(list.Count, Allocator.Temp);
                em.Instantiate(prefab, entities);

                for (var i = 0; i < entities.Length; i++) {
                    var creation = list[i];

                    em.SetComponentData(entities[i], LocalTransform.FromPositionRotationScale(
                        creation.Transform.Position,
                        quaternion.Euler(creation.Transform.Rotation),
                        creation.Transform.Scale
                    ));

                    em.SetComponentData(entities[i], new InstanceIndexComponent {
                        Value = creation.InstanceIndex
                    });
                }

                // Single SetSharedComponent call sets the chunk parent for
                // every entity in this preset group, in one structural change.
                em.SetSharedComponent(entities, new ChunkParentComponent { ChunkCoord = _id });

                entities.Dispose();
            }
        }
        
        public void OnInstancesDeleted(IReadOnlyList<int> instanceIndices) {
            if (instanceIndices.Count == 0)
                return;

            DestroyEntities(instanceIndices);

            foreach (var instanceIndex in instanceIndices)
                DeleteEventSurface(instanceIndex);
        }
        
        public void OnInstanceDataChanged(int instanceIndex, InstanceDataKey key) { }
        
        #endregion IObjectInstanceConsumer Implementation

        #region EventSurfaces
        
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
        
        private void DeleteEventSurface(int instanceIndex) {
            if (!_eventSurfaces.TryGetValue(instanceIndex, out var surface)) 
                return;

            UnityEngine.Object.Destroy(surface.gameObject);
            _eventSurfaces.Remove(instanceIndex);
        }

        #endregion EventSurfaces

        #region Distance Queries
        
        public void EntityDistanceQuery(Vector3 playerPosition) {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;

            var indices = _query.ToComponentDataArray<InstanceIndexComponent>(Allocator.Temp);
            var entities = _query.ToEntityArray(Allocator.Temp);
            var transforms = _query.ToComponentDataArray<LocalTransform>(Allocator.Temp);
            var thresholds = _query.ToComponentDataArray<StaticProximityObjectComponent>(Allocator.Temp);
            var presetUids = _query.ToComponentDataArray<PresetUidComponent>(Allocator.Temp);

            for (var i = 0; i < entities.Length; i++) {
                if (!em.Exists(entities[i]))
                    continue;

                var distance = Vector3.Distance(playerPosition, transforms[i].Position);
                var hasProxy = _eventSurfaces.ContainsKey(indices[i].Value);
                var preset = presetUids[i].Value.Value.ResolvePreset();

                if (distance < thresholds[i].Value.x && !hasProxy)
                    SpawnSurface(transforms[i], indices[i].Value, preset);
                else if (distance > thresholds[i].Value.y && hasProxy)
                    DespawnSurface(indices[i].Value);
            }

            indices.Dispose();
            entities.Dispose();
            transforms.Dispose();
            thresholds.Dispose();
            presetUids.Dispose();
        }
        
        #endregion Distance Queries

        #region IDisposable

        public void Dispose() {
            DestroyAllEntities();
        }

        #endregion IDisposable
    }
}