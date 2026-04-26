// Copyright 2026 Spellbound Studio Inc.

using UnityEngine;

namespace Spellbound.Core {
    /// <summary>
    /// Contract for registering and unregistering network governed objects.
    /// </summary>
    public interface IObjectParent {
        public ObjectParent ObjectParent { get; }
        void InitializeObjectParentChunk(Vector3Int id);
    }
}