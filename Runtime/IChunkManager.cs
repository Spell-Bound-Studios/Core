// Copyright 2025 Spellbound Studio Inc.

using System;
using UnityEngine;

namespace Spellbound.Core {
    /// <summary>
    /// IChunkManager is the responsible interface for providing a contractual binding back to a managers role to
    /// objects.
    /// </summary>
    public interface IChunkManager {
        public event Action<int> OnChunkCountChanged;

        public event Action<Vector3> OnPlayerPositionChanged;

        public IObjectParentChunk GetObjectParentChunk(Vector3 position);
    }
}