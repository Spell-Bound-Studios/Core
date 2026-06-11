// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using Spellbound.Core.Registries;
using UnityEngine;

namespace Spellbound.Core.Objects {
    /// <summary>
    /// Resolves <see cref="ObjectPreset"/>s by the stable FNV-1a hash of their asset GUID. Auto-discovers
    /// every preset under a Resources folder; a hash collision is a hard error (regenerate one preset's GUID).
    /// </summary>
    public static class PresetRegistry {
        private static readonly HashRegistry<ObjectPreset> Registry = new();
        private static bool _isLoaded;

        /// <summary>
        /// Every registered preset.
        /// </summary>
        public static IReadOnlyList<ObjectPreset> All {
            get {
                EnsureLoaded();

                return Registry.All;
            }
        }

        #region Lifecycle

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetForPlaySession() {
            Registry.Clear();
            _isLoaded = false;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void WarmUp() => EnsureLoaded();

        #endregion

        #region API

        /// <summary>
        /// The preset for a hash, or null.
        /// </summary>
        public static ObjectPreset ResolvePreset(uint hash) {
            EnsureLoaded();

            return Registry.TryGet(hash, out var preset) ? preset : null;
        }

        /// <summary>
        /// The preset for a hash; false if none is registered.
        /// </summary>
        public static bool TryResolvePreset(uint hash, out ObjectPreset preset) {
            EnsureLoaded();

            return Registry.TryGet(hash, out preset);
        }

        /// <summary>
        /// True if a preset with this hash is registered.
        /// </summary>
        public static bool Contains(uint hash) {
            EnsureLoaded();

            return Registry.Contains(hash);
        }

        #endregion

        #region Internal

        private static void EnsureLoaded() {
            if (_isLoaded)
                return;

            _isLoaded = true;

            foreach (var preset in Resources.LoadAll<ObjectPreset>("")) {
                if (Registry.Contains(preset.Hash))
                    throw new InvalidOperationException(
                        $"Preset hash collision: '{preset.objectName}' (asset '{preset.name}', guid {preset.Guid}) " +
                        $"collides at hash {preset.Hash}. Regenerate one preset's GUID to resolve.");

                Registry.Add(preset);
            }
        }

        #endregion
    }
}
