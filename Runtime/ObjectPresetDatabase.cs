// Copyright 2025 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using UnityEngine;

namespace Spellbound.Core {
    /// <summary>
    /// Global database for all objects.
    /// </summary>
    public sealed class ObjectPresetDatabase : MonoBehaviour {
        private readonly Dictionary<string, ObjectPreset> _presets = new();

        private void Awake() {
            DontDestroyOnLoad(gameObject);
            SingletonManager.RegisterSingleton(this);

            var presets = Resources.LoadAll<ObjectPreset>("");

            foreach (var preset in presets) {
                if (!_presets.TryAdd(preset.presetUid, preset))
                    Debug.LogError($"Duplicate procedural object uid {preset.presetUid}");
            }
        }

        // Public getter for non-Burst systems that still want the preset object.
        public bool TryGetPreset(string uid, out ObjectPreset preset) {
            // Guarantee preset is always assigned.
            if (!string.IsNullOrEmpty(uid))
                return _presets.TryGetValue(uid, out preset);

            preset = null;

            return false;
        }
        
        /// <summary>
        /// Called by the bootstrap system after baking to write entity prefab
        /// buffer indices back into each preset.
        /// </summary>
        public void RegisterEntityPrefabIndices(Dictionary<string, int> guidToBufferIndex) {
            foreach (var (uid, index) in guidToBufferIndex) {
                if (!_presets.TryGetValue(uid, out var preset)) {
                    Debug.LogWarning($"RegisterEntityPrefabIndices: no preset found for uid {uid}");
                    continue;
                }
                preset.entityPrefabBufferIndex = index;
            }
        }
    }
    
    

    
    public static class ObjectPresetUtils {
        
        public static ObjectPreset ResolvePreset(this string uid) =>
                !string.IsNullOrEmpty(uid) &&
                SingletonManager.TryGetSingletonInstance(out ObjectPresetDatabase db) &&
                db.TryGetPreset(uid, out var preset)
                        ? preset
                        : null;
        
        /// <summary>
        /// Resolves the entity prefab for this preset from the DOTS prefab registry.
        /// Returns Entity.Null if the index hasn't been registered yet.
        /// </summary>
        public static Entity GetEntityPrefab(this ObjectPreset preset) {
            if (preset.entityPrefabBufferIndex < 0) {
                Debug.LogWarning($"Entity prefab index not registered for preset {preset.presetUid}");
                return Entity.Null;
            }

            if (!SingletonManager.TryGetSingletonInstance(out EntityPrefabRegistry registry))
                return Entity.Null;

            return registry.GetPrefab(preset.entityPrefabBufferIndex);
        }
    }
}