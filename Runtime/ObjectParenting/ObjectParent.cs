using System;
using System.Collections.Generic;
using Spellbound.Core.ECS;
using Spellbound.Core.Packing;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Spellbound.Core {
    /// <summary>
    /// Poco1
    /// Poco to assist management of object data by the parent.
    /// </summary>
    public class ObjectParent {
        private readonly IObjectDataStore _dataStore;
        private Dictionary<int, EventSurface> _eventSurfaces;
        private EntityQuery _chunkEntityQuery;
        private ChunkParentComponent _chunkParentComponent;

        public ObjectParent(IObjectDataStore dataStore, Dictionary<int, EventSurface> eventSurfaces, Vector3Int key) {
            _dataStore = dataStore;
            _eventSurfaces = eventSurfaces;
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            _chunkEntityQuery = entityManager.CreateEntityQuery(typeof(ChunkParentComponent));
            _chunkParentComponent = new ChunkParentComponent {
                ChunkCoord = new int3(key.x, key.y, key.z)
            };

            _dataStore.OnInstanceRemoved += HandleInstanceRemoved;
        }

        public bool TryReadData<T>(int instanceIndex, string presetUid, Func<string, T> fallbackData, out T result) where T : IPacker, new() {
            PackerRegistry.TryGetId(typeof(T), out var packerId);

            if (packerId == null) {
                result = default;
                return false;
            }
            
            if (_dataStore.TryRead(instanceIndex, packerId, out var bytes)) {
                result = Packer.FromBytes<T>(bytes);
                Debug.Log($"result is {result}");
                return true;
            }
            
            // Data doesn't exist. Use fallback to generate defaults and write through store.
            if (fallbackData == null) {
                result = default;
                return false;
            }
            
            // Ensure the instance exists in the store before writing data to it.
            if (!_dataStore.HasInstance(instanceIndex))
                _dataStore.CreateInstance(instanceIndex, presetUid);
            
            result = fallbackData.Invoke(presetUid);
            _dataStore.Write(instanceIndex, packerId, Packer.ToBytes(result));
            return true;
        }
        
        public bool TryWriteData<T>(int instanceIndex, string presetUid, T value) where T : IPacker {
            PackerRegistry.TryGetId(typeof(T), out var packerId);

            if (packerId == null) 
                return false;

            if (!_dataStore.HasInstance(instanceIndex))
                _dataStore.CreateInstance(instanceIndex, presetUid);
            
            _dataStore.Write(instanceIndex, packerId, Packer.ToBytes(value));
            return true;
        }

        public bool TryTransformData<T>(
            int instanceIndex, string presetUid, Func<string, T> fallbackFunc, T delta) where T : IQuantitativeData{
            PackerRegistry.TryGetId(typeof(T), out var packerId);
            
            Debug.Log("PackerId: " + packerId);

            if (packerId == null) {
  
                return false;
            }
            var fallbackData = fallbackFunc.Invoke("my seed");
            _dataStore.Delta(instanceIndex, presetUid, packerId, Packer.ToBytes(delta), Packer.ToBytes(fallbackData));
            
            return true;
        }
        
        public bool TryDeleteData(int instanceIndex) => _dataStore.TryDeleteInstance(instanceIndex);

        private void HandleInstanceRemoved(int instanceIndex) {
            DeleteEntity(instanceIndex);
            DeleteEventSurface(instanceIndex);
        }

        private void DeleteEventSurface(int instanceIndex) {
            if (!_eventSurfaces.TryGetValue(instanceIndex, out var surface)) {
                return;
            }
            UnityEngine.Object.Destroy(surface.gameObject);
            _eventSurfaces.Remove(instanceIndex);
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
        
        private static string GetPackerId<T>() {
            var id = PackerIdCache<T>.Id;

            if (id == null)
                Debug.LogWarning($"[ObjectParent] {typeof(T).Name} is missing a PackerIdAttribute.");

            return id;
        }
    }
}