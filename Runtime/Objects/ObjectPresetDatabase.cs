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
    }
    
    

    
    public static class ObjectPresetUtils {
        public static ObjectPreset ResolvePreset(this string uid) =>
                !string.IsNullOrEmpty(uid) &&
                SingletonManager.TryGetSingletonInstance(out ObjectPresetDatabase db) &&
                db.TryGetPreset(uid, out var preset)
                        ? preset
                        : null;
        
        public static bool TryGetEntityPrefab(this string uid, out Entity entity) {
            if (!SingletonManager.TryGetSingletonInstance(out EntityPrefabRegistry registry)) {
                Debug.LogWarning("EntityPrefabRegistry not found");
                entity = Entity.Null;
                return false;
            }

            if (!registry.PrefabLookup.TryGetValue(uid, out entity)) {
                Debug.LogWarning($"No entity bakePrefab found for preset {uid.ResolvePreset()}");
                return false;
            }

            return true;
        }
    }
}