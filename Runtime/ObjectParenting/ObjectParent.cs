using System.Collections.Generic;
using Spellbound.Core.Packing;

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

        public bool TryReadData<T>(int instanceIndex, out T result) where T : IPacker, new() {
            var packerId = GetPackerId<T>();

            if (packerId == null) {
                result = default;

                return false;
            }
            if (!_dataStore.TryGetInstanceBag(instanceIndex, out var bag)) {

            }
            if (!bag.TryRead(packerId, out var bytes)){
                
                
            }

            result = Packer.FromBytes<T>(bytes);
            return true;
        }
        
        public bool TryWriteData<T>(int instanceIndex, T value) where T : IPacker {
            var packerId = GetPackerId<T>();

            if (packerId == null) 
                return false;

            _dataStore.WriteInstanceData(instanceIndex, packerId, Packer.ToBytes(value));
            return true;
        }
        
        private static string GetPackerId<T>() {
            var id = PackerIdCache<T>.Id;

            if (id == null)
                UnityEngine.Debug.LogWarning($"[ObjectParent] {typeof(T).Name} is missing a PackerIdAttribute.");

            return id;
        }
        
        #region Raw Access

        public bool TryGetInstanceBag(int instanceIndex, out InstanceDataBag bag)
            => _dataStore.TryGetInstanceBag(instanceIndex, out bag);

        public IEnumerable<(int instanceIndex, InstanceDataBag bag)> GetAllBags()
            => _dataStore.GetAllBags();

        public IEnumerable<(int instanceIndex, InstanceDataBag bag)> GetDirtyBags()
            => _dataStore.GetDirtyBags();

        public bool HasInstance(int instanceIndex)
            => _dataStore.HasInstance(instanceIndex);

        public void WriteInstanceData(int instanceIndex, string packerId, byte[] data)
            => _dataStore.WriteInstanceData(instanceIndex, packerId, data);

        #endregion
    }
}