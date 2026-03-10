

namespace Spellbound.Core {
    /// <summary>
    /// Poco1
    /// Poco to assist management of object data by the parent. 
    /// </summary>
    public class ObjectParent {
        private IObjectDataStore _dataStore;
        public ObjectParent(IObjectDataStore datastore) {
            _dataStore = datastore;
        }
    }
}