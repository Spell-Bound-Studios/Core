// Copyright 2025 Spellbound Studio Inc.

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

        public GameObject prefab; // not the proxy, it is the thing that bakes into an entity
        public EventSurface eventSurfacePrefab;
        public float interactionDistance = 50;

        [SerializeReference]
        public List<PresetModule> modules = new();

        public bool TryGetModule<T>(out T result) where T : PresetModule {
            foreach (var pm in modules) {
                if (pm is T t) {
                    result = t;
                    return true;
                }
            }

            result = null;
            return false;
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