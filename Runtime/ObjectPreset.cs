// Copyright 2025 Spellbound Studio Inc.

using System.Collections.Generic;
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

        [SerializeReference] public List<PresetModule> modules = new();

#if UNITY_EDITOR
        /// <summary>
        /// Creates guids based on an asset path for us when something gets updated.
        /// </summary>
        private void OnValidate() {
            var assetPath = UnityEditor.AssetDatabase.GetAssetPath(this);

            if (assetPath == null) {
                presetUid = string.Empty;

                return;
            }

            var assetGuid = UnityEditor.AssetDatabase.GUIDFromAssetPath(assetPath).ToString();
            if (string.IsNullOrEmpty(presetUid) || presetUid != assetGuid) 
                presetUid = assetGuid;
        }
#endif
    }
}