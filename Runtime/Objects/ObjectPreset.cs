// Copyright 2025 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Spellbound.Core {
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

        public GameObject bakePrefab; // not the proxy, it is the thing that bakes into an entity
        public EventSurface eventSurfacePrefab;
        public float interactionDistance = 50;
        
        [SerializeField]
        public List<PresetSurface> surfaceModules = new();

        /// <summary>
        /// Searches ALL surfaces for a module of type T. Returns the first match.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="surfaceIndex"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool TryGetModule<T>(out T result, int surfaceIndex = 0) where T : PresetModule {
            result = null;
            if (surfaceIndex < 0 || surfaceIndex >= surfaceModules.Count)
                return false;

            foreach (var pm in surfaceModules[surfaceIndex].presetModules) {
                if (pm is not T t) 
                    continue;

                result = t;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Searches ALL surfaces for a module matching a runtime Type. Returns the first match.
        /// </summary>
        /// <param name="moduleType"></param>
        /// <param name="result"></param>
        /// <param name="surfaceIndex"></param>
        /// <returns></returns>
        public bool TryGetModule(Type moduleType, out PresetModule result, int surfaceIndex = 0) {
            result = null;
            if (surfaceIndex < 0 || surfaceIndex >= surfaceModules.Count)
                return false;

            foreach (var pm in surfaceModules[surfaceIndex].presetModules) {
                if (!moduleType.IsAssignableFrom(pm.GetType())) 
                    continue;

                result = pm;
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// Returns all modules across all surfaces.
        /// </summary>
        public IEnumerable<PresetModule> GetAllModules() {
            foreach (var surface in surfaceModules)
            foreach (var pm in surface.presetModules)
                if (pm != null)
                    yield return pm;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Creates guids based on an asset path for us when something gets updated.
        /// </summary>
        private void OnValidate() {
            var assetPath = AssetDatabase.GetAssetPath(this);

            if (assetPath == null) {
                presetUid = string.Empty;

                return;
            }

            var assetGuid = AssetDatabase.GUIDFromAssetPath(assetPath).ToString();
            if (string.IsNullOrEmpty(presetUid) || presetUid != assetGuid) 
                presetUid = assetGuid;
            
            EditorUtility.SetDirty(this);
        }
#endif
    }
}