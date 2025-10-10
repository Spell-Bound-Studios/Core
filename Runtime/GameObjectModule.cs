// Copyright 2025 Spellbound Studio Inc.

using System;
using UnityEngine;

namespace SpellBound.Core {
    [Serializable]
    public class GameObjectModule : PresetModule {
        public GameObject prefab;
        public float defaultScale = 1;

        public override SbbData? GetData(ObjectPreset preset) => null;
    }
}