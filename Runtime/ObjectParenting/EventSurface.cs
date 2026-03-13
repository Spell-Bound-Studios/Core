using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Spellbound.Core {
    [RequireComponent(typeof(Collider))]
    public class EventSurface : MonoBehaviour {
        private IObjectParent _parent;
        private int _entityIndex;
        private ObjectPreset _objectPreset;

        public void Initialize(IObjectParent parent, int entityIndex, string presetUid) {
            _parent = parent;
            _entityIndex = entityIndex;
            _objectPreset = presetUid.ResolvePreset();
        }

        public void DebugQueryPing() {
            Debug.Log($"pinging Event Surface for {_objectPreset.name} index {_entityIndex}");
        }
        // Declare a THandler type at runtime that will pass in a pointer of that type to THAT types implementation.
        public void Dispatch<THandler>(Action<THandler, IObjectParent, int, string> invoke) where THandler : class {
            if (_objectPreset == null) 
                return;
            
            foreach (var module in _objectPreset.modules)
                if (module is THandler handler) 
                    invoke(handler, _parent, _entityIndex, _objectPreset.presetUid);
        }
    }
}