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
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Spellbound.Core {
    /// <summary>
    /// Poco to assist management of object data by the parent.
    /// </summary>
    public class ObjectParent : IDisposable, IObjectInstanceConsumer {
        private readonly IObjectParent _implementer;
        private readonly Transform _transform;

        private readonly int3 _id;
        private EntityQuery _query;
        private readonly Entity _ecsChunk;

        private readonly Dictionary<int, IEventSurface> _eventSurfaces = new();
        private Vector3 _lastPovPosition;

        public IObjectDataAccess DataAccess { get; }

        /// <summary>
        /// Constructor. Includes the creation of ready-made entity queries.
        /// </summary>
        /// <param name="implementer"></param>
        /// <param name="transform"></param>
        /// <param name="dataAccess"></param>
        /// <param name="parentId"></param>
        /// <param name="ecsChunk"></param>
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
                where T : IDecodableData, new() {
            if (DataAccess.TryRead<T>(instanceIndex, eventSurfaceIndex, out var data)) {
                result = data;

                return true;
            }

            result = default;

            return false;
        }
        
        public bool TryReadDataAllData(int instanceIndex, string presetUid, int eventSurfaceIndex, out List<IDecodableData> results) {
            return DataAccess.TryReadAll(instanceIndex, eventSurfaceIndex, out results);
        }

        public bool TryWriteData<T>(int instanceIndex, string presetUid, int eventSurfaceIndex, T newData, byte context = 0)
                where T : IDecodableData, new() {
            DataAccess.Write(instanceIndex, presetUid, eventSurfaceIndex, newData, context);

            return true;
        }

        public bool TryTransformData<T>(
            int instanceIndex, string presetUid, int eventSurfaceIndex, T delta) where T : IQuantitativeData, new() {

            DataAccess.Delta(instanceIndex, presetUid, eventSurfaceIndex, delta);

            return true;
        }

        public async Task<bool> TryDeleteData(int instanceIndex) => await DataAccess.TryDeleteInstance(instanceIndex);

        #endregion API

        #region ECS

        /// <summary>
        /// Indicates to the ECS world that the Chunk is ready for the Instantiation System to make entities.
        /// </summary>
        public void SetEcsChunkReady() {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            em.AddComponentData(_ecsChunk, new ReadyForObjectsTag());
        }

        /// <summary>
        /// Sends the full state as Entity Spawn Requests to the ECS world.
        /// Note: event surfaces will not be added immediately.
        /// </summary>
        public void BufferFullStateStaticInstances() {
            var instances = DataAccess.GetAllRuntimeInstances();

            if (instances.Count == 0)
                return;

            BufferEntitySpawnRequests(instances.Select(kvp => (kvp.Key, kvp.Value)));
        }

        /// <summary>
        /// Sends the full state as Deletions to the ECS world.
        /// These are all the procedurally-generated objects that the save state knows are deleted.
        /// These will be skipped over when the Instantiation System instantiates entities.
        /// </summary>
        public void BufferFullStateDeletions() {
            var deletions = DataAccess.GetAllSeedInstanceDeletions();

            if (deletions.Count == 0)
                return;

            var em = World.DefaultGameObjectInjectionWorld.EntityManager;

            var buffer = em.HasBuffer<DeletionBufferElement>(_ecsChunk)
                    ? em.GetBuffer<DeletionBufferElement>(_ecsChunk)
                    : em.AddBuffer<DeletionBufferElement>(_ecsChunk);

            foreach (var deletion in deletions)
                buffer.Add(new DeletionBufferElement { Value = deletion });
        }

        /// <summary>
        /// Destroys Entities from a list. 
        /// </summary>
        /// <param name="instanceIndices"></param>
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

        /// <summary>
        /// Destroys ALL entities, including the _ecsChunk itself,
        /// by making a new query that only queries for the ChunkParentComponent.
        /// </summary>
        private void DestroyAllEntities() {
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

        private void BufferEntitySpawnRequests(IEnumerable<(int, NonProceduralStaticInstanceEntry)> instances) {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;

            var buffer = em.HasBuffer<EntitySpawnRequestElement>(_ecsChunk)
                    ? em.GetBuffer<EntitySpawnRequestElement>(_ecsChunk)
                    : em.AddBuffer<EntitySpawnRequestElement>(_ecsChunk);

            foreach (var (instanceIndex, entry) in instances) {
                if (!entry.PresetUid.TryGetEntityPrefab(out var prefab)) {
                    Log.Error($"Entity prefab could not be found: {entry.PresetUid}");

                    continue;
                }

                buffer.Add(new EntitySpawnRequestElement {
                    Prefab = prefab,
                    InstanceIndex = instanceIndex,
                    Transform = entry.Transform.ToLocalTransform()
                });
            }
        }

        #endregion ECS

        #region IObjectInstanceConsumer Implementation

        /// <summary>
        /// Handles the in-game consequences Instances being loaded.
        /// Buffers them to be created as entities,
        /// and spawns an EventSurface for those who pass the proximymath check. 
        /// </summary>
        /// <param name="instances"></param>
        public void OnRuntimeInstancesLoaded(IReadOnlyList<(int, NonProceduralStaticInstanceEntry)> instances) {
            BufferEntitySpawnRequests(instances);

            foreach (var (instanceIndex, entry) in instances) {
                var preset = entry.PresetUid.ResolvePreset();

                var proximityChange = ProximityMath.IsWithinActivationRange(entry.Transform.Position, _lastPovPosition,
                    preset.interactionDistance);

                if (proximityChange == ProximityChange.Whitelist) SpawnSurface(preset, entry.Transform, instanceIndex);
            }
        }

        /// <summary>
        /// Handles the in-game consequences Instances being loaded.
        /// Buffers them to be created as entities,
        /// and for those that pass the proximitymath check, spawns an EventSurface and calls ICreationHandler's OnCreation actions.
        /// </summary>
        /// <param name="instances"></param>
        public void OnRuntimeInstancesCreated(IReadOnlyList<(int, NonProceduralStaticInstanceEntry)> instances) {
            BufferEntitySpawnRequests(instances);

            foreach (var (instanceIndex, entry) in instances) {
                var preset = entry.PresetUid.ResolvePreset();

                var proximityChange = ProximityMath.IsWithinActivationRange(entry.Transform.Position, _lastPovPosition,
                    preset.interactionDistance);

                if (proximityChange == ProximityChange.Whitelist) {
                    SpawnSurface(preset, entry.Transform, instanceIndex);

                    if (!preset.TryGetModule(typeof(ICreationHandler), out var module)) {
                        return; // safe return
                    }
                    
                    if (module is ICreationHandler handler) {
                        handler.OnCreation(instanceIndex, this, out var actions);

                        foreach (var action in actions) {
                            action.Invoke(instanceIndex, entry.Transform);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles the in-game consequences of an Instances being deleted.
        /// Destroys their associated entities,
        /// Destroys their associated EventSurfaces if they exist.
        /// And calls IDestructionHandler's OnDestruction actions.
        /// </summary>
        /// <param name="instanceIndices"></param>
        public void OnInstancesDeleted(IReadOnlyList<int> instanceIndices) {
            if (instanceIndices.Count == 0)
                return;

            foreach (var instanceIndex in instanceIndices) {
                if (!_eventSurfaces.TryGetValue(instanceIndex, out var surface)) {
                    continue;
                }

                if (surface == null) {
                    Log.Error($"surface is null");
                    continue;
                }

                if (!surface.Preset.TryGetModule(typeof(IDestuctionHandler), out var module)) {
                    continue; // safe return
                }

                if (module is IDestuctionHandler handler) {
                    handler.OnDestruction(instanceIndex, this, out var actions);

                    foreach (var action in actions) {
                        action.Invoke(instanceIndex, new TransformData(surface.Transform));
                    }
                }
                
                DespawnSurface(instanceIndex);
            } 
                

            DestroyEntities(instanceIndices);
        }

        public void OnInstancesDeleteResolve(IReadOnlyList<int> instanceIndices) {
            
        }

        public void OnInstanceDataChanged(int instanceIndex, InstanceDataKey key, IDecodableData data, byte context) {
            if (!TryGetCallbackParamsFromEventSurface(instanceIndex, key.SurfaceIndex, out var transformData,
                    out var preset, out IEventSurface surface)) {
                return;
            }
            surface.AlertChanged();

            data.InvokeChangeCallback(context, DataAccess, instanceIndex, preset, key.SurfaceIndex,
                transformData);
        }
        
        public void OnInstanceDataResolved(int instanceIndex, InstanceDataKey key, IDecodableData data, byte context) {
            if (!TryGetCallbackParamsFromEventSurface(instanceIndex, key.SurfaceIndex, out var transformData,
                    out var preset, out IEventSurface surface)) {
                return;
            }
            
            if (!TryGetCallbackParamsFromEntity(instanceIndex, out transformData, out preset)) {
                Log.Error("failed to find entity");
                return;
            }
            

            data.InvokeResolveCallback(context, DataAccess, instanceIndex, preset, key.SurfaceIndex,
                transformData);
        }


        /// <summary>
        /// /// Helper Method to get Transform and Preset from Event Surface
        /// </summary>
        /// <param name="instanceIndex"></param>
        /// <param name="surfaceIndex"></param>
        /// <param name="transformData"></param>
        /// <param name="preset"></param>
        /// <param name="surface"></param>
        /// <returns></returns>
        private bool TryGetCallbackParamsFromEventSurface(int instanceIndex, int surfaceIndex, out TransformData transformData, out ObjectPreset preset, out IEventSurface surface) {
            surface = null;
            transformData = default;
            preset = null;
            if (!_eventSurfaces.TryGetValue(instanceIndex, out var mainSurface)) {
                return false;
            }

            if (!mainSurface.TryGetEventSurfaceByIndex(surfaceIndex, out surface)) {
                Log.Error($"Surface not found for instanceIndex {instanceIndex} and surfaceIndex  {surfaceIndex}");
            }
            
            transformData = new TransformData(surface.Transform);
            preset = surface.Preset;

            return true;
        }
        
        /// <summary>
        /// Helper Method to get Transform and Preset from Entity
        /// </summary>
        /// <param name="instanceIndex"></param>
        /// <param name="transformData"></param>
        /// <param name="preset"></param>
        /// <returns></returns>
        private bool TryGetCallbackParamsFromEntity(int instanceIndex, out TransformData transformData, out ObjectPreset preset) {
            transformData = default;
            preset = null;
            var instanceIndices = _query.ToComponentDataArray<InstanceIndexComponent>(Allocator.Temp);
            var entities = _query.ToEntityArray(Allocator.Temp);
            
            for (var i = 0; i < instanceIndices.Length; i++) {
                if (instanceIndices[i].Value != instanceIndex) {
                    continue;
                }
                    
                var em  = World.DefaultGameObjectInjectionWorld.EntityManager;

                transformData = new TransformData(em.GetComponentData<LocalTransform>(entities[i]));
                preset = em.GetSharedComponentManaged<PresetUidComponent>(entities[i]).Value.Value.ResolvePreset();

                instanceIndices.Dispose();
                entities.Dispose();
                return true;
            }

            instanceIndices.Dispose();
            entities.Dispose();
            return false;
        }

        #endregion IObjectInstanceConsumer Implementation

        #region EventSurfaces

        private void SpawnSurface(ObjectPreset preset, TransformData transformData, int instanceIndex) {
            if (_eventSurfaces.ContainsKey(instanceIndex)) {
                Log.Error($"Instance index {instanceIndex} already a key in eventSurfacesDictionary");

                return;
            }
            var eventSurface = UnityEngine.Object.Instantiate(
                preset.eventSurfacePrefab,
                transformData.Position,
                transformData.RotAsQuaternion(),
                _transform
            ).GetComponent<IEventSurface>();
            eventSurface.GameObject.name = $"{preset.name} Event Surface {instanceIndex}";
            eventSurface.GameObject.transform.localScale = transformData.ScaleAsVector3();
            eventSurface.Initialize(_implementer, instanceIndex, preset.presetUid);
            _eventSurfaces[instanceIndex] = eventSurface;
        }

        /// <summary>
        /// Destroys an event surface if it exists.
        /// Note remove is allowed to fail safely.
        /// </summary>
        /// <param name="instanceIndex"></param>
        private void DespawnSurface(int instanceIndex) {
            if (!_eventSurfaces.Remove(instanceIndex, out var surface)) 
                return;
            
            UnityEngine.Object.Destroy(surface.GameObject);
        }

        #endregion EventSurfaces

        #region Distance Queries

        public void StaticEntityDistanceQuery(float3 localPov) {
            _lastPovPosition = localPov;
            var existingEventSurfaces = new NativeHashSet<int>(_eventSurfaces.Count, Allocator.TempJob);

            foreach (var key in _eventSurfaces.Keys)
                existingEventSurfaces.Add(key);

            var maxCapacity = _query.CalculateEntityCount();
            var entities = _query.ToEntityArray(Allocator.TempJob);
            var transforms = _query.ToComponentDataArray<LocalTransform>(Allocator.TempJob);
            var thresholds = _query.ToComponentDataArray<StaticProximityObjectComponent>(Allocator.TempJob);
            var instanceIndices = _query.ToComponentDataArray<InstanceIndexComponent>(Allocator.TempJob);
            var povPositions = new NativeArray<float3>(1, Allocator.TempJob);
            povPositions[0] = localPov;

            var instancesToAwaken = new NativeParallelHashSet<int>(maxCapacity, Allocator.TempJob);
            var instancesToSleep = new NativeParallelHashSet<int>(maxCapacity, Allocator.TempJob);

            var proximityJob = new StaticEventSurfaceProximityJob {
                PovPositions = povPositions,
                Transforms = transforms,
                Thresholds = thresholds,
                InstanceIndices = instanceIndices,
                ExistingEventSurfaces = existingEventSurfaces,
                InstancesToAwaken = instancesToAwaken.AsParallelWriter(),
                InstancesToSleep = instancesToSleep.AsParallelWriter()
            };
            var jobHandle = proximityJob.Schedule(maxCapacity, 64);
            jobHandle.Complete();

            var em = World.DefaultGameObjectInjectionWorld.EntityManager;

            foreach (var toAwaken in instancesToAwaken) {
                var presetUid = em.GetSharedComponent<PresetUidComponent>(entities[toAwaken]);

                SpawnSurface(presetUid.Value.Value.ResolvePreset(), new TransformData(transforms[toAwaken]),
                    instanceIndices[toAwaken].Value);
            }

            foreach (var toSleep in instancesToSleep) 
                DespawnSurface(instanceIndices[toSleep].Value); 

            povPositions.Dispose();
            transforms.Dispose();
            thresholds.Dispose();
            instanceIndices.Dispose();
            entities.Dispose();
            existingEventSurfaces.Dispose();
            instancesToAwaken.Dispose();
            instancesToSleep.Dispose();
        }

        #endregion Distance Queries

        #region IDisposable

        public void Dispose(){
            DestroyAllEntities();
        } 

        #endregion IDisposable
    }
}