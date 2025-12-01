// Copyright 2025 Spellbound Studio Inc.

using System;
using UnityEngine;

namespace Spellbound.Core.ECS {
    /// <summary>
    /// EntityModule is an essential script for the ECS portion of Core. It allows PresetModules to have EntityModule
    /// components in the inspector and will register them with systems that anticipate this module.
    /// Add an EntityModule to your Preset if you want your prefab available to be spawned as an entity.
    /// </summary>
    [Serializable]
    public class EntityModule : PresetModule {
        public GameObject entityPrefab;
        public GameObject proxyColliderObj;

        public override SbbData? GetData(ObjectPreset preset) => null;
    }
}