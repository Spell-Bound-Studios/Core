// Copyright 2026 Spellbound Studio Inc.

using System.Collections.Generic;

namespace Spellbound.Core.Registries {
    /// <summary>
    /// A lookup of <typeparamref name="TEntry"/> by stable uint hash. This is the surface consumers depend on;
    /// the backend behind it (the in-memory <see cref="HashRegistry{TEntry}"/>, or a future database) can be
    /// swapped without touching callers.
    /// </summary>
    public interface IRegistry<TEntry> where TEntry : class {
        bool TryGet(uint hash, out TEntry entry);
        TEntry Get(uint hash);
        bool Contains(uint hash);
        int Count { get; }
        IReadOnlyList<TEntry> All { get; }
    }
}
