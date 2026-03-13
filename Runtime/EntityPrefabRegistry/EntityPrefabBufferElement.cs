// Copyright 2026 Spellbound Studio Inc.

using Unity.Collections;
using Unity.Entities;

namespace Spellbound.Core {
    /// <summary>
    /// Buffer for entity prefabs and their guid.
    /// </summary>
    public struct EntityPrefabBufferElement : IBufferElementData {
        public Entity Prefab;
        public FixedString64Bytes Guid;
    }
}