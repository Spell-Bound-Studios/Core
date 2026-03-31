using System;
using UnityEngine;

namespace Spellbound.Core {
    [RequireComponent(typeof(Collider))]
    public class EventSurface : MonoBehaviour {
        [SerializeField, Tooltip("Decide your own surface index schema.")] private int surfaceIndex = -1;
        public Vector3 Position => transform.position;
        
        private IObjectParent _parent;
        private int _entityIndex;
        private ObjectPreset _objectPreset;
        
        public void Initialize(IObjectParent parent, int entityIndex, string presetUid) {
            _parent = parent;
            _entityIndex = entityIndex;
            _objectPreset = presetUid.ResolvePreset();
            
            // Check for children.
        }

        public void DebugQueryPing() {
            Debug.Log($"Pinging Event Surface for {_objectPreset.name} " +
                      $"index {_entityIndex} " +
                      $"and surface index {surfaceIndex}");
        }
        
        // Declare a THandler type at runtime that will pass in a pointer of that type to THAT types implementation.
        public void Dispatch<THandler>(Action<THandler, IObjectParent, int, string> invoke) where THandler : class {
            if (_objectPreset == null) 
                return;
            
            /*foreach (var module in _objectPreset.modules)
                if (module is THandler handler) 
                    invoke(handler, _parent, _entityIndex, _objectPreset.presetUid);*/

            // If this event surface doesn't have children - early return.
            if (surfaceIndex < 0 /*|| surfaceIndex >= _objectPreset.modules.Count*/)
                return;
            
            // If it does have children loop through them and invoke.
            foreach (var module in _objectPreset.surfaceModules[surfaceIndex].PresetModules)
                if (module is THandler handler)
                    invoke(handler, _parent, _entityIndex, _objectPreset.presetUid);
        }
    }
}