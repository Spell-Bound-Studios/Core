// Copyright 2026 Spellbound Studio Inc.

using System;

namespace Spellbound.Core.Packing {
    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
    public sealed class PackerIdAttribute : Attribute {
        public string Id { get; }

        public PackerIdAttribute(string id) {
            Id = id;
        }
    }
}