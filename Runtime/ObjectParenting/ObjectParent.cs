using System;
using System.Collections.Generic;
using Spellbound.Core.Packing;
using UnityEngine;

namespace Spellbound.Core {
    /// <summary>
    /// Poco1
    /// Poco to assist management of object data by the parent.
    /// </summary>
    public class ObjectParent {
        private readonly IObjectDataStore _dataStore;

        public ObjectParent(IObjectDataStore datastore) {
            _dataStore = datastore;
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
                return true;
            }
            return false;
        }

        private void HandleInstanceDeleted(int instanceIndex) {
            // deletes the entity and the event-surface representations of the object
        }
        
        public static string GetPackerId<T>() {
            var id = PackerIdCache<T>.Id;

            if (id == null)
                UnityEngine.Debug.LogWarning($"[ObjectParent] {typeof(T).Name} is missing a PackerIdAttribute.");

            return id;
        }
    }
}