// Copyright 2026 Spellbound Studio Inc.

namespace Spellbound.Core.Hashing {
    /// <summary>
    /// Canonical hashing for stable, deterministic ids. FNV-1a (offset 2166136261, prime 16777619) over a
    /// string produces the same value on every machine and build. Use this anywhere a string needs a compact,
    /// stable numeric id (registry keys, save / network ids) instead of re-implementing the algorithm inline.
    /// </summary>
    public static class StableHash {
        public const uint Fnv1aOffset = 2166136261u;
        public const uint Fnv1aPrime = 16777619u;

        /// <summary>
        /// Full 32-bit FNV-1a of <paramref name="value"/>. Empty or null returns 0, which callers treat as the
        /// reserved "null / none" id.
        /// </summary>
        public static uint Fnv1a32(string value) {
            if (string.IsNullOrEmpty(value))
                return 0u;

            var hash = Fnv1aOffset;

            for (var i = 0; i < value.Length; i++) {
                hash ^= value[i];
                hash *= Fnv1aPrime;
            }

            return hash;
        }

        /// <summary>
        /// 16-bit variant: the 32-bit hash folded down with XOR. Matches the value PackerRegistry computed
        /// inline before this util existed.
        /// </summary>
        public static ushort Fnv1a16(string value) {
            var hash = Fnv1a32(value);

            return (ushort)(hash ^ (hash >> 16));
        }
    }
}
