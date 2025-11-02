// Copyright 2025 Spellbound Studio Inc.

using System.Collections.Generic;
using UnityEngine;

namespace Spellbound.Core.Console {
    /// <summary>
    /// Registry that maps preset object names to their UIDs.
    /// Enables console commands to resolve "apple" → presetUid.
    /// </summary>
    public static class PresetConsoleRegistry {
        private static readonly Dictionary<string, string> NameToUid = new();
        private static bool _isInitialized = false;

        /// <summary>
        /// Initializes the preset registry by scanning all ObjectPresets with ConsoleModules.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize() {
            if (_isInitialized)
                return;

            RegisterAllPresets();
            _isInitialized = true;
        }

        /// <summary>
        /// Scans all ObjectPresets in Resources and registers any with ConsoleModules.
        /// </summary>
        public static void RegisterAllPresets() {
            var allPresets = Resources.LoadAll<ObjectPreset>("");
            var registeredCount = 0;

            foreach (var preset in allPresets) {
                if (!preset.TryGetModule<ConsoleModule>(out var consoleModule)) 
                    continue;
                
                if (!consoleModule.autoRegister) 
                    continue;

                RegisterPreset(preset);
                registeredCount++;
            }

            Debug.Log($"[PresetConsoleRegistry] Registered {registeredCount} console-accessible presets");
        }

        /// <summary>
        /// Registers a single preset's objectName → presetUid mapping.
        /// </summary>
        private static void RegisterPreset(ObjectPreset preset) {
            var key = preset.objectName.ToLower();

            if (NameToUid.ContainsKey(key))
                Debug.LogWarning(
                    $"[PresetConsoleRegistry] Duplicate preset name '{preset.objectName}' - overwriting previous registration");

            NameToUid[key] = preset.presetUid;
        }

        /// <summary>
        /// Attempts to resolve a preset name to its UID.
        /// </summary>
        /// <param name="objectName">The preset's objectName (case-insensitive).</param>
        /// <param name="presetUid">The resolved preset UID, or null if not found.</param>
        /// <returns>True if the preset was found, false otherwise.</returns>
        public static bool TryResolvePresetUid(string objectName, out string presetUid) {
            if (!_isInitialized)
                Initialize();

            return NameToUid.TryGetValue(objectName.ToLower(), out presetUid);
        }

        /// <summary>
        /// Clears all registered presets (for testing purposes).
        /// </summary>
        public static void Clear() {
            NameToUid.Clear();
            _isInitialized = false;
        }
    }
}