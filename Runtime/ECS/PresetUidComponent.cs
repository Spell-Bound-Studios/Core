// Copyright 2026 Spellbound Studio Inc.

using System;
using Unity.Collections;
using Unity.Entities;

namespace Spellbound.Core.ECS {
    /// <summary>
    /// PresetUidComponent is an essential script for the ECS portion of Core. It is the ECS equivalent of the
    /// MonoBehaviour SpellboundBehaviour and exists as the ECS analogue.
    /// </summary>
    public struct PresetUidComponent : ISharedComponentData, IEquatable<PresetUidComponent> {
        public FixedString64Bytes Value;

        public bool Equals(PresetUidComponent other) => Value.Equals(other.Value);

        public override bool Equals(object obj) => obj is PresetUidComponent other && Equals(other);

        public override int GetHashCode() => Value.GetHashCode();
    }
}