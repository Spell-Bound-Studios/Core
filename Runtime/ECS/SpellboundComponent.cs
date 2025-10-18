// Copyright 2025 Spellbound Studio Inc.

using Unity.Collections;
using Unity.Entities;

namespace Spellbound.Core {
    public struct SpellboundComponent : IComponentData {
        public FixedString64Bytes PresetUiD;
        public int GenerationIndex;
    }
}