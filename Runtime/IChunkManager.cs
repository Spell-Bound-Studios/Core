// Copyright 2025 Spellbound Studio Inc.

using System;
using UnityEngine;

namespace Spellbound.Core {
    public interface IChunkManager {
        public event Action<int> OnChunkCountChanged;

        public event Action<Vector3> OnPlayerPositionChanged;

        public IObjectParentChunk GetObjectParentChunk(Vector3 position);
    }
}