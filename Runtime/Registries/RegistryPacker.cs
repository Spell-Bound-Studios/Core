// Copyright 2026 Spellbound Studio Inc.

using System;
using Spellbound.Core.Packing;

namespace Spellbound.Core.Registries {
    /// <summary>
    /// Writes a registry entry as its 4-byte stable hash and resolves it back through a registry on read — the
    /// "send one id, get the entry" round-trip for saves and network. A null entry or a 0 hash round-trips to
    /// null.
    /// </summary>
    public static class RegistryPacker {
        public static void Write(ref Span<byte> buffer, IRegistryEntry entry) =>
                Packer.WriteUInt(ref buffer, entry != null ? entry.Hash : 0u);

        public static TEntry Read<TEntry>(ref ReadOnlySpan<byte> buffer, IRegistry<TEntry> registry)
                where TEntry : class {
            var hash = Packer.ReadUInt(ref buffer);

            if (hash == 0u || registry == null)
                return null;

            return registry.TryGet(hash, out var entry)
                    ? entry
                    : null;
        }
    }
}
