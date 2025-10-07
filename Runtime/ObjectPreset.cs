using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace SpellBound.Core {
    [CreateAssetMenu(fileName = "Object Preset", menuName = "Spellbound/Presets/ObjectPreset")]
    public class ObjectPreset : ScriptableObject {
        [Immutable] public string presetUid;
        public string objectName;
        public string objectDescription;
        
        [SerializeReference] public List<PresetModule> modules = new();
        
#if UNITY_EDITOR
        /// <summary>
        /// Creates guids based on asset path for us when something gets updated.
        /// </summary>
        private void OnValidate() {
            var assetPath = UnityEditor.AssetDatabase.GetAssetPath(this);

            if (assetPath == null) {
                presetUid = string.Empty;
                return;
            }
            
            var assetGuid = UnityEditor.AssetDatabase.GUIDFromAssetPath(assetPath).ToString();
            if (string.IsNullOrEmpty(presetUid) || presetUid != assetGuid) {
                presetUid = assetGuid;
            }
        }
#endif
    }
}