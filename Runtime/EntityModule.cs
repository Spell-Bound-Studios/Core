using System;
using SpellBound.Core;
using UnityEngine;

namespace SpellBound.Core {
    [Serializable]
    public class EntityModule : PresetModule {
        public GameObject entityPrefab;
        public GameObject proxyColliderObj;

        public override SbbData? GetData(ObjectPreset preset) => null;
    }
}