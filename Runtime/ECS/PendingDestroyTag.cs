// Copyright 2025 Spellbound Studio Inc.

using Unity.Entities;

namespace Spellbound.Core {
    /// <summary>
    /// Tag for Entities to be destroyed
    /// </summary>
    public struct PendingDestroyTag : IComponentData { }
}