// Copyright 2026 Spellbound Studio Inc.

using System.Reflection;

namespace Spellbound.Core.Packing {
    public static class PackerIdCache<T> {
        public static readonly string Id = typeof(T).GetCustomAttribute<PackerIdAttribute>()?.Id;
    }
}