using System;
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
        private EntityQuery _chunkEntityQuery;
        private ChunkParentComponent _chunkParentComponent;

        public ObjectParent(IObjectDataStore datastore, Vector3Int key) {
            _dataStore = datastore;
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            _chunkEntityQuery = entityManager.CreateEntityQuery(typeof(ChunkParentComponent));
            _chunkParentComponent = new ChunkParentComponent {
                ChunkCoord = new int3(key.x, key.y, key.z)
            };
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

            if (!_dataStore.HasInstance(instanceIndex))
                _dataStore.CreateInstanceDataBag(instanceIndex, presetuid);
            
            _dataStore.WriteInstanceData(instanceIndex, packerId, Packer.ToBytes(value));
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
            
            _dataStore.WriteInstanceData(instanceIndex, packerId, Packer.ToBytes(result));
            return true;
        }
        
        public bool TryDeleteData(int instanceIndex) {
            if (_dataStore.TryDeleteInstance(instanceIndex)) {
                HandleInstanceDeleted(instanceIndex);
                return true;
            }
            return false;
        }

        private void HandleInstanceDeleted(int instanceIndex) {
            DeleteEntity(instanceIndex);
            DeleteEventSurface(instanceIndex);
        }

        private void DeleteEventSurface(int instanceIndex) {
            
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
                Debug.LogWarning($"[ObjectParent] {typeof(T).Name} is missing a PackerIdAttribute.");

            return id;
        }
    }
}