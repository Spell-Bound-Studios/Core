// Copyright 2026 Spellbound Studio Inc.

using Unity.Collections;
using Unity.Entities;

namespace Spellbound.Core.ECS {
    /// <summary>
    /// PresetUidComponent is an essential script for the ECS portion of Core. It is the ECS equivalent of the
    /// MonoBehaviour SpellboundBehaviour and exists as the ECS analogue.
    /// </summary>
    public struct PresetUidComponent : IComponentData {
        public FixedString64Bytes Value;
    }
}