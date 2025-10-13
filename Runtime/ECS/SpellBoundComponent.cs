// Copyright 2025 Spellbound Studio Inc.

using Unity.Collections;
using Unity.Entities;

namespace SpellBound.Core {
    public struct SpellBoundComponent : IComponentData {
        public FixedString64Bytes PresetUiD;
        public int GenerationIndex;
    }
}