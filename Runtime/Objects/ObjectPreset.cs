// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using Spellbound.Core.Logging;
using Spellbound.Core.Modules;
using Spellbound.Core.PresetContracts;
using Spellbound.Core.Surfaces;
using Spellbound.Core.Tooling;
using UnityEditor;
using UnityEngine;

namespace Spellbound.Core.Objects {
    /// <summary>
    /// ObjectPreset is the root of most "objects" that a player would want in game. It comes equipped with some baseline
    /// utility but also offers a modules field where additional modules that inherit from PresetModule can be loaded
    /// like MonoBehaviours.
    /// </summary>
    [CreateAssetMenu(fileName = "Object Preset", menuName = "Spellbound/Presets/ObjectPreset")]
    public class ObjectPreset : ScriptableObject {
        [Immutable] public string presetUid;
        public string objectName;
        public string objectDescription;
        public bool isDynamic;

        public GameObject bakePrefab; // not the proxy, it is the thing that bakes into an entity
        public GameObject eventSurfacePrefab;
        public Vector2 interactionDistance = new(50, 70);

        [SerializeField] public List<PresetSurface> surfaceModules = new();
        
        // In ObjectPreset
        private void OnEnable() => RewireModules();

        private void RewireModules() {
            if (surfaceModules == null)
                return;

            for (var i = 0; i < surfaceModules.Count; i++) {
                var surface = surfaceModules[i];

                if (surface?.presetModules == null)
                    continue;

                foreach (var module in surface.presetModules)
                    module?.OnPresetLoaded(this, i);
            }
        }

        public bool TryGetModule<T>(out T result, int surfaceIndex = 0) where T : class {
            if (TryGetModules<T>(out var results, surfaceIndex)) {
                result = results[0];
                return true;
            }

            result = null;
            return false;
        }

        public bool TryGetModules<T>(out IReadOnlyList<T> results, int surfaceIndex = 0) where T : class {
            results = Array.Empty<T>();

            if (surfaceIndex < 0 || surfaceIndex >= surfaceModules.Count)
                return false;

            var matches = new List<T>();
            foreach (var module in surfaceModules[surfaceIndex].presetModules) {
                if (module is T t)
                    matches.Add(t);
            }

            if (matches.Count == 0)
                return false;

            results = matches;

            return true;
        }
        
        public bool TryGetModulesAcrossSurfaces<T>(out IReadOnlyList<T>  results) where T : class {
            results = Array.Empty<T>();
            List<T> matches = null;

            for (var i = 0; i < surfaceModules.Count; i++) {
                var surface = surfaceModules[i];

                if (surface?.presetModules == null)
                    continue;

                foreach (var module in surface.presetModules) {
                    if (module is not T t)
                        continue;

                    matches ??= new List<T>();
                    matches.Add(t);
                }
            }

            if (matches == null)
                return false;

            results = matches;
            return true;
        }

        /// <summary>
        /// Returns all modules across all surfaces.
        /// </summary>
        public IEnumerable<PresetModule> GetAllModules() {
            foreach (var surface in surfaceModules)
            foreach (var pm in surface.presetModules) {
                if (pm != null)
                    yield return pm;
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// Creates guids based on an asset path for us when something gets updated.
        /// </summary>
        private void OnValidate() {
            if (eventSurfacePrefab != null && eventSurfacePrefab.GetComponent<IEventSurface>() == null)
                Log.Error("EventSurfacePrefab not found");

            //Prohibits Multiple IDispatch<T> of the same T, and multiple ITooltipHandler of any T or no T at all.
            foreach (var surface in surfaceModules) {
                if (surface.presetModules == null) continue;

                var seenDispatchTypes = new HashSet<Type>();
                var hasMouseoverHandler = false;

                foreach (var module in surface.presetModules) {
                    if (module == null) continue;

                    // Enforce: only one IDispatch<T> per T per surface
                    foreach (var iface in module.GetType().GetInterfaces()) {
                        if (!iface.IsGenericType) continue;
                        if (iface.GetGenericTypeDefinition() != typeof(IDispatch<>)) continue;

                        if (!seenDispatchTypes.Add(iface)) {
                            Log.Error(
                                $"Duplicate {iface.Name}<{iface.GenericTypeArguments[0].Name}> " +
                                $"on surface '{surface.surfaceName}' in preset '{name}'");
                        }
                    }

                    // Enforce: only one ITooltipHandler of any kind per surface
                    if (module is ITooltipHandler) {
                        if (hasMouseoverHandler) {
                            Debug.LogError(
                                $"Duplicate ITooltipHandler on surface '{surface.surfaceName}' in preset '{name}'");
                        }

                        hasMouseoverHandler = true;
                    }
                }
            }

            var assetPath = AssetDatabase.GetAssetPath(this);

            if (assetPath == null) {
                presetUid = string.Empty;

                return;
            }

            var fileName = System.IO.Path.GetFileNameWithoutExtension(assetPath);

            if (!string.IsNullOrEmpty(fileName) && name != fileName)
                name = fileName;

            var assetGuid = AssetDatabase.GUIDFromAssetPath(assetPath).ToString();

            if (string.IsNullOrEmpty(presetUid) || presetUid != assetGuid)
                presetUid = assetGuid;

            EditorUtility.SetDirty(this);
        }
#endif
    }
}