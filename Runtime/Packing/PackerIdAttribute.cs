// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Reflection;

namespace Spellbound.Core.Packing {
    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
    public sealed class PackerIdAttribute : Attribute {
        public string Id { get; }
        public PackerIdAttribute(string id) => Id = id;
    }

    public static class PackerIdCache<T> {
        public static readonly string Id = typeof(T).GetCustomAttribute<PackerIdAttribute>()?.Id;
    }
}