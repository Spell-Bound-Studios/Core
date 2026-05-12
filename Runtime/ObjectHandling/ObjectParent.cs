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
        private EntityQuery _staticQuery;
        private EntityQuery _dynamicQuery;
        private readonly Entity _ecsChunk;

        private readonly Dictionary<int, IEventSurface> _eventSurfaces = new();
        private Vector3 _lastPovPosition;

        public IObjectDataAccess StaticDataAccess { get; }
        public IDynamicDataAccess DynamicDataAccess { get; }
        
        private int _seedInstanceCount;

        public event Action<float3[]> OnDynamicProximityEval = delegate { };

        /// <summary>
        /// Constructor. Includes the creation of ready-made entity queries.
        /// </summary>
        /// <param name="implementer"></param>
        /// <param name="transform"></param>
        /// <param name="staticDataAccess"></param>
        /// <param name="dynamicDataAccess"></param>
        /// <param name="parentId"></param>
        /// <param name="ecsChunk"></param>
        public ObjectParent(
            IObjectParent implementer, Transform transform, IObjectDataAccess staticDataAccess, IDynamicDataAccess dynamicDataAccess, Vector3Int parentId,
            Entity ecsChunk) {
            _implementer = implementer;
            _transform = transform;
            _id = new int3(parentId.x, parentId.y, parentId.z);
            _ecsChunk = ecsChunk;

            var em = World.DefaultGameObjectInjectionWorld.EntityManager;

            _staticQuery = em.CreateEntityQuery(
                ComponentType.ReadOnly<ChunkParentComponent>(),
                ComponentType.ReadOnly<LocalTransform>(),
                ComponentType.ReadOnly<ProximityThresholdComponent>(),
                ComponentType.ReadOnly<InstanceIndexComponent>(),
                ComponentType.ReadOnly<PresetUidComponent>(),
                ComponentType.Exclude<DynamicTag>()
            );

            _dynamicQuery = em.CreateEntityQuery(
                ComponentType.ReadOnly<ChunkParentComponent>(),
                ComponentType.ReadOnly<LocalTransform>(),
                ComponentType.ReadOnly<ProximityThresholdComponent>(),
                ComponentType.ReadOnly<InstanceIndexComponent>(),
                ComponentType.ReadOnly<PresetUidComponent>(),
                ComponentType.ReadOnly<DynamicTag>()
                );

            _staticQuery.SetSharedComponentFilter(new ChunkParentComponent {
                ChunkCoord = _id
            });
            
            _dynamicQuery.SetSharedComponentFilter(new ChunkParentComponent {
                ChunkCoord = _id
            });

            StaticDataAccess = staticDataAccess;
            StaticDataAccess.SetConsumer(this);

            DynamicDataAccess = dynamicDataAccess;
            DynamicDataAccess.SetConsumer(this);
        }

        #region API
        
        public void SetSeedInstanceCount(int seedCount) => _seedInstanceCount = seedCount;

        public void Instantiate(
            ObjectPreset preset, Vector3 position, Vector3 rotation, int scale, List<(InstanceDataKey, byte[])> dataSlots = null) {
            if (!preset.isDynamic) {
                StaticDataAccess.CreateRuntimeInstance(preset.presetUid, position, rotation, scale);
                return;
            }

            DynamicDataAccess.CreateRuntimeObject(preset.presetUid, position, rotation, scale, dataSlots);
        }
        
        public void AdoptMigratedDynamic(int newIndex, DynamicInstanceEntry entry, IEventSurface surface) {
            surface.Transform.SetParent(_transform, true);
            surface.Initialize(_implementer, newIndex, entry.PresetUid);
        }

        public void CreateNewInstance(ObjectPreset preset, Vector3 position, Vector3 rotation, int scale) {
            if (!preset.isDynamic) {
                StaticDataAccess.CreateRuntimeInstance(preset.presetUid, position, rotation, scale);
                return;
            }

            DynamicDataAccess.CreateRuntimeObject(preset.presetUid, position, rotation, scale);
        }
                

        public bool TryReadData<T>(int instanceIndex, string presetUid, int eventSurfaceIndex, out T result)
                where T : IPacker, new() {
            if (StaticDataAccess.TryRead<T>(instanceIndex, eventSurfaceIndex, out var data)) {
                result = data;
                Debug.Log($"result is {result}");

                return true;
            }

            result = default;

            return false;
        }

        public bool TryWriteData<T>(int instanceIndex, string presetUid, int eventSurfaceIndex, T newData)
                where T : IPacker, new() {
            StaticDataAccess.Write(instanceIndex, presetUid, eventSurfaceIndex, newData);

            return true;
        }

        public bool TryTransformData<T>(
            int instanceIndex, string presetUid, int eventSurfaceIndex, T delta) where T : IQuantitativeData, new() {
            Debug.Log($"Calling TryTransformData on instanceIndex {instanceIndex}, delta {delta}");

            StaticDataAccess.Delta(instanceIndex, presetUid, eventSurfaceIndex, delta);

            return true;
        }

        public async Task<bool> TryDeleteData(int instanceIndex) => await StaticDataAccess.TryDeleteInstance(instanceIndex);

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
            var instances = StaticDataAccess.GetAllRuntimeInstances();

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
            var deletions = StaticDataAccess.GetAllSeedInstanceDeletions();

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
        /// Destroys Static Entities from a list. 
        /// </summary>
        /// <param name="instanceIndices"></param>
        private void DestroyStaticEntities(IReadOnlyList<int> instanceIndices) {
            var removedSet = new HashSet<int>(instanceIndices);

            var indices = _staticQuery.ToComponentDataArray<InstanceIndexComponent>(Allocator.Temp);
            var entities = _staticQuery.ToEntityArray(Allocator.Temp);
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
            disposeQuery.SetSharedComponentFilter(new ChunkParentComponent {
                ChunkCoord = _id
            });

            var entities = disposeQuery.ToEntityArray(Allocator.Temp);
            em.DestroyEntity(entities);
            entities.Dispose();
        }

        /// <summary>
        /// Overload for static entries created.
        /// </summary>
        /// <param name="instances"></param>
        private void BufferEntitySpawnRequests(IEnumerable<(int, NonProceduralStaticInstanceEntry)> instances) {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;

            var buffer = em.HasBuffer<EntitySpawnRequestElement>(_ecsChunk)
                    ? em.GetBuffer<EntitySpawnRequestElement>(_ecsChunk)
                    : em.AddBuffer<EntitySpawnRequestElement>(_ecsChunk);

            foreach (var (instanceIndex, entry) in instances) {
                if (!entry.PresetUid.TryGetEntityPrefab(out var prefab)) {
                    Log.Error($"Entity prefab could not be found for preset: {entry.PresetUid}");
                    continue;
                }

                buffer.Add(new EntitySpawnRequestElement {
                    Prefab = prefab,
                    InstanceIndex = instanceIndex,
                    Transform = entry.Transform.ToLocalTransform()
                });
            }
        }

        /// <summary>
        /// Overload for dynamic requests converting to entities.
        /// </summary>
        /// <param name="requests"></param>
        private void BufferEntitySpawnRequests(IReadOnlyList<(int, string, TransformData)> requests) {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;

            var buffer = em.HasBuffer<EntitySpawnRequestElement>(_ecsChunk)
                    ? em.GetBuffer<EntitySpawnRequestElement>(_ecsChunk)
                    : em.AddBuffer<EntitySpawnRequestElement>(_ecsChunk);

            foreach (var (instanceIndex, presetUid, transformData) in requests) {
                if (!presetUid.TryGetEntityPrefab(out var prefab)) {
                    Log.Error($"Entity prefab could not be found for preset: {presetUid}");
                    continue;
                }

                buffer.Add(new EntitySpawnRequestElement {
                    Prefab = prefab,
                    InstanceIndex = instanceIndex,
                    Transform = transformData.ToLocalTransform()
                });
            }
        }

        #endregion ECS

        #region IObjectInstanceConsumer Implementation

        public int GetNextInstanceIndex() {
            var i = _seedInstanceCount;
            while (StaticDataAccess.HasInstance(i) || DynamicDataAccess.HasInstance(i))
                i++;
            return i;
        }

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

                if (proximityChange == ProximityChange.Whitelist)
                    SpawnSurface(preset, entry.Transform, instanceIndex);
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

                if (proximityChange != ProximityChange.Whitelist)
                    continue;

                SpawnSurface(preset, entry.Transform, instanceIndex);

                if (!preset.TryGetModule(typeof(ICreationHandler), out var module))
                    return;

                if (module is not ICreationHandler handler)
                    continue;

                handler.OnCreation(instanceIndex, this, out var actions);

                foreach (var action in actions)
                    action.Invoke(instanceIndex, entry.Transform);
            }
        }

        public void OnDynamicObjectsLoaded(IReadOnlyList<(int, DynamicInstanceEntry)> loaded) {
            
        }
        
        public void OnDynamicObjectsCreated(IReadOnlyList<(int, DynamicInstanceEntry)> creations) {
            foreach (var (instanceIndex, entry) in creations) {
                var preset = entry.PresetUid.ResolvePreset();

                if (preset == null || preset.eventSurfacePrefab == null) {
                    Log.Error($"OnDynamicInstancesCreated: missing preset or prefab for {entry.PresetUid}");
                    continue;
                }
                
                var instance = UnityEngine.Object.Instantiate(
                    preset.eventSurfacePrefab.gameObject,
                    entry.Transform.Position,
                    entry.Transform.RotAsQuaternion(),
                    _transform);

                if (instance.TryGetComponent<IEventSurface>(out var surface))
                    surface.Initialize(_implementer, instanceIndex, entry.PresetUid);
            }
        }

        public void OnDynamicObjectEntityRequested(IReadOnlyList<(int, string, TransformData)> entityRequests) =>
            BufferEntitySpawnRequests(entityRequests);

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
                if (!_eventSurfaces.TryGetValue(instanceIndex, out var surface)) 
                    continue;

                if (surface == null) {
                    Log.Error($"surface is null");
                    continue;
                }

                if (!surface.Preset.TryGetModule(typeof(IDestuctionHandler), out var module)) 
                    continue; // safe return

                if (module is IDestuctionHandler handler) {
                    handler.OnDestruction(instanceIndex, this, out var actions);

                    foreach (var action in actions) action.Invoke(instanceIndex, new TransformData(surface.Transform));
                }

                DespawnSurface(instanceIndex);
            }

            DestroyStaticEntities(instanceIndices);
        }

        /// <summary>
        /// Handles the in-game structural changes of data changing.
        /// Structural means it's going to create or destroy instance(s) or make changes to another instance.
        /// It will call the IThresholdHandler's actions if it's passed a threshold.
        /// And call the IChangeHandler's OnChangeStructural actions.
        /// An example of a Threshold check and action is for something "dying";
        /// he threshold is at 0 hp and the action is death or deletion
        /// </summary>
        /// <param name="instanceIndex"></param>
        /// <param name="key"></param>
        /// <param name="dataFunc"></param>
        /// <param name="handlerType"></param>
        public void OnInstanceDataStructuralChanged(
            int instanceIndex, InstanceDataKey key, Func<IPacker> dataFunc, Type handlerType) {
            if (!typeof(IChangeHandler).IsAssignableFrom(handlerType) &&
                !typeof(IThresholdHandler).IsAssignableFrom(handlerType))
                return;

            if (!TryGetCallbackParamsFromEventSurface(instanceIndex, out var transformData, out var preset)) {
                if (!TryGetCallbackParamsFromEntity(instanceIndex, out transformData, out preset)) {
                    Log.Error($"Neither event surface nor entity could be found for instanceIndex {instanceIndex}");

                    return;
                }
            }

            if (!preset.TryGetModule(handlerType, out var module, key.SurfaceIndex)) {
                Log.Error($"Preset does not have expected module for preset {preset}");

                return;
            }

            var data = dataFunc();

            if (module is IThresholdHandler handler && data is IQuantitativeData quantitativeData) {
                if (handler.ThresholdCheck(quantitativeData, this, out var actions)) {
                    foreach (var action in actions)
                        action.Invoke(instanceIndex, transformData);

                    return;
                }
            }

            if (module is IChangeHandler changeHandler) {
                changeHandler.OnChangeStructural(data, instanceIndex, this, out var actions);

                foreach (var action in actions)
                    action.Invoke(instanceIndex, transformData);
            }
        }

        /// <summary>
        /// Handles the in-game cosmetic changes of data changings.
        /// Cosmetic is defined in contrast to structural.
        /// Cosmetic means it's just visual and auditory "juice" to make the changes noticable and game-feely.
        /// Calls the IChangeHandler's OnChangeCosmetic Actions.
        /// </summary>
        /// <param name="instanceIndex"></param>
        /// <param name="key"></param>
        /// <param name="dataFunc"></param>
        /// <param name="handlerType"></param>
        public void OnInstanceDataCosmeticChanged(
            int instanceIndex, InstanceDataKey key, Func<IPacker> dataFunc, Type handlerType) {
            // TODO: Call an event here for tooltips to subscribe to keep their data from going stale if they are open
            // TODO while data continues to change.

            if (!typeof(IChangeHandler).IsAssignableFrom(handlerType))
                return;

            if (!TryGetCallbackParamsFromEventSurface(instanceIndex, out var transformData, out var preset)) return;

            if (!preset.TryGetModule(handlerType, out var module, key.SurfaceIndex)) {
                Log.Error($"Preset does not have expected module for preset {preset}");

                return;
            }

            var data = dataFunc();

            if (module is not IChangeHandler changeHandler) 
                return;

            changeHandler.OnChangeCosmetic(data, instanceIndex, this, out var actions);

            foreach (var action in actions)
                action.Invoke(instanceIndex, transformData);
        }

        /// <summary>
        /// Handles the in-game consequences of an instances Data being set.
        /// This is very similar to OnRuntimeInstancesLoaded, but for seeded instances.
        /// The method is currently empty because there are as of yet no consequences for this.
        /// </summary>
        /// <param name="instanceIndex"></param>
        /// <param name="key"></param>
        /// <param name="dataFunc"></param>
        public void OnInstanceDataInitialized(
            int instanceIndex, InstanceDataKey key, Func<IPacker> dataFunc) { }

        /// <summary>
        /// Helper Method to get Transform and Preset from Event Surface
        /// </summary>
        /// <param name="instanceIndex"></param>
        /// <param name="transformData"></param>
        /// <param name="preset"></param>
        /// <returns></returns>
        private bool TryGetCallbackParamsFromEventSurface(
            int instanceIndex, out TransformData transformData, out ObjectPreset preset) {
            transformData = default;
            preset = null;

            if (!_eventSurfaces.TryGetValue(instanceIndex, out var surface)) return false;

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
        private bool TryGetCallbackParamsFromEntity(
            int instanceIndex, out TransformData transformData, out ObjectPreset preset) {
            transformData = default;
            preset = null;
            var instanceIndices = _staticQuery.ToComponentDataArray<InstanceIndexComponent>(Allocator.Temp);
            var entities = _staticQuery.ToEntityArray(Allocator.Temp);

            for (var i = 0; i < instanceIndices.Length; i++) {
                if (instanceIndices[i].Value != instanceIndex) continue;

                var em = World.DefaultGameObjectInjectionWorld.EntityManager;

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
        
        public void SleepDynamicObject(int instanceIndex, DynamicInstanceEntry entry) => DynamicDataAccess.Sleep(instanceIndex, entry);
        
        #endregion EventSurfaces

        #region Distance Queries
        
        public void DynamicDistanceQuery(float3[] povs) {
            OnDynamicProximityEval?.Invoke(povs);
            
            var maxCapacity = _dynamicQuery.CalculateEntityCount();
            
            if (maxCapacity == 0) 
                return;
            
            var existingEventSurfaces = new NativeHashSet<int>(1, Allocator.TempJob);
            
            var entities = _dynamicQuery.ToEntityArray(Allocator.TempJob);
            var transforms = _dynamicQuery.ToComponentDataArray<LocalTransform>(Allocator.TempJob);
            var thresholds = _dynamicQuery.ToComponentDataArray<ProximityThresholdComponent>(Allocator.TempJob);
            var instanceIndices = _dynamicQuery.ToComponentDataArray<InstanceIndexComponent>(Allocator.TempJob);
            var povPositions = new NativeArray<float3>(povs, Allocator.TempJob);
            
            var instancesToAwaken = new NativeParallelHashSet<int>(maxCapacity, Allocator.TempJob);
            var instancesToSleep = new NativeParallelHashSet<int>(maxCapacity, Allocator.TempJob);

            var proximityJob = new EventSurfaceProximityJob {
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

            foreach (var toAwaken in instancesToAwaken) {
                if (DynamicDataAccess.TryAwaken(toAwaken))
                    continue;

                Log.Error($"Tried to awaken an object with index {toAwaken} that does not exist.");
                return;
            }

            povPositions.Dispose();
            transforms.Dispose();
            thresholds.Dispose();
            instanceIndices.Dispose();
            entities.Dispose();
            existingEventSurfaces.Dispose();
            instancesToAwaken.Dispose();
            instancesToSleep.Dispose();
        }

        public void StaticEntityDistanceQuery(float3 localPov) {
            _lastPovPosition = localPov;
            var existingEventSurfaces = new NativeHashSet<int>(_eventSurfaces.Count, Allocator.TempJob);

            foreach (var key in _eventSurfaces.Keys)
                existingEventSurfaces.Add(key);

            var maxCapacity = _staticQuery.CalculateEntityCount();
            var entities = _staticQuery.ToEntityArray(Allocator.TempJob);
            var transforms = _staticQuery.ToComponentDataArray<LocalTransform>(Allocator.TempJob);
            var thresholds = _staticQuery.ToComponentDataArray<ProximityThresholdComponent>(Allocator.TempJob);
            var instanceIndices = _staticQuery.ToComponentDataArray<InstanceIndexComponent>(Allocator.TempJob);
            var povPositions = new NativeArray<float3>(1, Allocator.TempJob);
            povPositions[0] = localPov;

            var instancesToAwaken = new NativeParallelHashSet<int>(maxCapacity, Allocator.TempJob);
            var instancesToSleep = new NativeParallelHashSet<int>(maxCapacity, Allocator.TempJob);

            var proximityJob = new EventSurfaceProximityJob {
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

        public void Dispose() => DestroyAllEntities();

        #endregion IDisposable
    }
}