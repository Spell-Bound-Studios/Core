// Copyright 2026 Spellbound Studio Inc.

using System.Collections.Generic;
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
}