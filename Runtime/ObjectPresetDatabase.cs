using System;
using System.Collections.Generic;
using PurrNet;
using UnityEngine;
using System.Linq;

namespace SpellBound.Core {
    /// <summary>
    /// Global database for all objects.
    /// </summary>
    public sealed class ObjectPresetDatabase : MonoBehaviour {
        private readonly Dictionary<string, ObjectPreset> _presets = new();
        private readonly Dictionary<(string uid, Type moduleType), PresetModule> _moduleLookup = new();
        
        private void Awake() {
            DontDestroyOnLoad(gameObject);
            InstanceHandler.RegisterInstance(this);
            
            var presets = Resources.LoadAll<ObjectPreset>("");
            foreach (var preset in presets) {
                if (!_presets.TryAdd(preset.presetUid, preset))
                    Debug.LogError($"Duplicate procedural object uid {preset.presetUid}");
                
                foreach (var module in preset.modules) {
                    if (module == null) 
                        continue;
                    
                    var key = (preset.presetUid, module.GetType());
                    
                    // This should never happen but regardless.
                    if (!_moduleLookup.TryAdd(key, module))
                        Debug.LogWarning(
                            $"{preset.name} already has a {module.GetType().Name}; second copy ignored",
                            preset);
                }
            }
        }

        private void OnDestroy() {
            InstanceHandler.UnregisterInstance<ObjectPresetDatabase>();
        }
        
        // Public getter for non-Burst systems that still want the preset object.
        public bool TryGetPreset(string uid, out ObjectPreset preset) {
            // Guarantee preset is always assigned.
            if (!string.IsNullOrEmpty(uid)) 
                return _presets.TryGetValue(uid, out preset);
            
            preset = null;
            return false;
        }
    }
    
    public static class ObjectPresetUtils {
        /// <summary>
        /// Returns true and the first module of type T if it exists on preset otherwise false.
        /// </summary>
        public static bool TryGetModule<T>(this ObjectPreset preset, out T module) where T : PresetModule {
            if (preset != null) {
                module = preset.modules.OfType<T>().FirstOrDefault();
                return module != null;
            }

            module = null;
            return false;
        }
        
        public static ObjectPreset ResolvePreset(this string uid) {
            return !string.IsNullOrEmpty(uid) &&
                   InstanceHandler.TryGetInstance(out ObjectPresetDatabase db) &&
                   db.TryGetPreset(uid, out var preset)
                ? preset
                : null;
        }
    }
}