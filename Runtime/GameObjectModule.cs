// Copyright 2025 Spellbound Studio Inc.

using System;
using UnityEngine;

namespace Spellbound.Core {
    /// <summary>
    /// GameObjectModule is an essential script for the PresetModule part of Core. It allows PresetModules to have
    /// GameObjectModule components in the inspector and will register them with systems that anticipate this module.
    /// Add a GameObjectModule to your Preset if you expect your preset to behave as a normal GameObject in the world.
    /// </summary>
    [Serializable]
    public class GameObjectModule : PresetModule {
        public GameObject prefab;
        public float defaultScale = 1;

        public override SbbData? GetData(ObjectPreset preset) => null;
    }
}