using System;
using System.Collections.Generic;
using Spellbound.Core.ECS;
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
    public class ObjectParent {
        private readonly IObjectDataStore _dataStore;
        private Dictionary<int, EventSurface> _eventSurfaceDict = new();
        private EntityQuery _chunkEntityQuery;
        private ChunkParentComponent _chunkParentComponent;
        private Transform _transform;
        private int proceduralInstanceCount;

        public ObjectParent(IObjectDataStore datastore, Vector3Int key, Transform transform) {
            _dataStore = datastore;
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            _chunkEntityQuery = entityManager.CreateEntityQuery(typeof(ChunkParentComponent));
            _chunkParentComponent = new ChunkParentComponent {
                ChunkCoord = new int3(key.x, key.y, key.z)
            };
            _transform = transform;
        }
        
        public void SetProceduralInstanceCount(int count) {
            proceduralInstanceCount = count;
        }

        // This provides a check to see if an instance deletion should trigger a "GoDeletion".
        private bool IsProcedural(int instanceIndex) {
            return instanceIndex < proceduralInstanceCount;
        }

        public void EntityDistanceQuery(Vector3 playerPosition) {
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            _chunkEntityQuery.SetSharedComponentFilter(_chunkParentComponent);
            using var entities = _chunkEntityQuery.ToEntityArray(Allocator.Temp);
            
            foreach (var entity in entities) {
                var eTransform = entityManager.GetComponentData<LocalTransform>(entity);
                float distance = Vector3.Distance(playerPosition, eTransform.Position);
                var spellboundComponent = entityManager.GetComponentData<SpellboundComponent>(entity);
                var hasProxy = _eventSurfaceDict.ContainsKey(spellboundComponent.GenerationIndex);
                var objectPreset = spellboundComponent.PresetUiD.Value.ResolvePreset();
                var iobjectParent = _transform.GetComponent<IObjectParent>();
                
    
                if (distance < objectPreset.interactionDistance && !hasProxy) {
                    var uid = entityManager.GetComponentData<SpellboundComponent>(entity).PresetUiD;
                    var preset = uid.Value.ResolvePreset();
                    var proxy = UnityEngine.Object.Instantiate(
                        preset.eventSurfacePrefab, eTransform.Position, eTransform.Rotation, _transform);
                    proxy.gameObject.name = $"{preset.name} Event Surface {spellboundComponent.GenerationIndex}";
                    proxy.transform.localScale = eTransform.Scale * Vector3.one;
                    proxy.Initialize(iobjectParent, spellboundComponent.GenerationIndex, uid.Value);
                    _eventSurfaceDict[spellboundComponent.GenerationIndex] = proxy;
                }
                else if (distance > objectPreset.interactionDistance + 10f && hasProxy) {

                    if (_eventSurfaceDict.TryGetValue(spellboundComponent.GenerationIndex, out var proxy)) {
                        UnityEngine.Object.Destroy(proxy.gameObject);
                        _eventSurfaceDict.Remove(spellboundComponent.GenerationIndex);
                    }
                }
            }
        }
        

        public bool TryReadData<T>(int instanceIndex, string presetuid, Func<string, T> fallbackData, out T result) 
                where T : IPacker, new() {
            var packerId = GetPackerId<T>();

            if (packerId == null) {
                result = default;

                return false;
            }
            if (!_dataStore.TryGetInstanceBag(instanceIndex, out var bag)) {
                bag = _dataStore.CreateInstanceDataBag(instanceIndex, presetuid);
            }
            if (!bag.TryRead(packerId, out var bytes)) {
                result = fallbackData.Invoke("My seed");
                bag.Write(packerId, Packer.ToBytes(result));
                return true;
            }

            result = Packer.FromBytes<T>(bytes);
            return true;
        }
        
        public bool TryWriteData<T>(int instanceIndex, string presetuid, T value) where T : IPacker {
            var packerId = GetPackerId<T>();

            if (packerId == null) 
                return false;

            if (!_dataStore.TryGetInstanceBag(instanceIndex, out var bag)) {
                bag = _dataStore.CreateInstanceDataBag(instanceIndex, presetuid);
            }
            bag.Write(packerId, Packer.ToBytes(value));
            return true;
        }

        public bool TryTransformData<T>(
            int instanceIndex, string presetuid, Func<string, T> fallbackData, Func<T, T> transformationFunc,
            out T result) where T : IPacker, new(){
            var packerId = GetPackerId<T>();

            if (packerId == null) {
                result = default;

                return false;
            }
            if (!_dataStore.TryGetInstanceBag(instanceIndex, out var bag)) {
                bag = _dataStore.CreateInstanceDataBag(instanceIndex, presetuid);
            }
            if (!bag.TryRead(packerId, out var bytes)) {
                result = transformationFunc(fallbackData.Invoke("My seed"));
            }
            else {
                result = transformationFunc(Packer.FromBytes<T>(bytes));
            }
            
            bag.Write(packerId, Packer.ToBytes(result));
            return true;
        }
        
        public bool TryDeleteData(int instanceIndex) {
            if (_dataStore.TryDeleteInstanceBag(instanceIndex)) {
                HandleInstanceDeleted(instanceIndex);
                return true;
            }
            return false;
        }

        private void HandleInstanceDeleted(int instanceIndex) {
            DeleteEntity(instanceIndex);
            DeleteEventSurface(instanceIndex);
            
            if (IsProcedural(instanceIndex))
                AddGoDeletion(instanceIndex);
        }

        private void DeleteEventSurface(int instanceIndex) {
            if (_eventSurfaceDict.TryGetValue(instanceIndex, out var proxy)) {
                UnityEngine.Object.Destroy(proxy.gameObject);
                _eventSurfaceDict.Remove(instanceIndex);
            }
        }

        private void AddGoDeletion(int instanceIndex) {
            Debug.Log("Adding Go Deletion is not implemented yet");
        }

        private void DeleteEntity(int instanceIndex) {
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            _chunkEntityQuery.SetSharedComponentFilter(_chunkParentComponent);
            using var entities = _chunkEntityQuery.ToEntityArray(Allocator.Temp);

            foreach (var entity in entities) {
                var entityGenIndex = entityManager.GetComponentData<SpellboundComponent>(entity).GenerationIndex;

                if (entityGenIndex != instanceIndex)
                    continue;

                entityManager.DestroyEntity(entity);

                break;
            }
        }
        
        public static string GetPackerId<T>() {
            var id = PackerIdCache<T>.Id;

            if (id == null)
                UnityEngine.Debug.LogWarning($"[ObjectParent] {typeof(T).Name} is missing a PackerIdAttribute.");

            return id;
        }
    }
}