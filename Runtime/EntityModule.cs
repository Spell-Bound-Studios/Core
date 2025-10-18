// Copyright 2025 Spellbound Studio Inc.

using System;
using UnityEngine;

namespace Spellbound.Core {
    [Serializable]
    public class EntityModule : PresetModule {
        public GameObject entityPrefab;
        public GameObject proxyColliderObj;

        public override SbbData? GetData(ObjectPreset preset) => null;
    }
}