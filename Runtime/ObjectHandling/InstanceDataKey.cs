// Copyright 2026 Spellbound Studio Inc.

using System;

namespace Spellbound.Core {
    public readonly struct InstanceDataKey : IEquatable<InstanceDataKey> {
        public readonly string PackerId;
        public readonly int SurfaceIndex;

        public InstanceDataKey(string packerId, int surfaceIndex) {
            PackerId = packerId;
            SurfaceIndex = surfaceIndex;
        }

        public bool Equals(InstanceDataKey other) => PackerId == other.PackerId && SurfaceIndex == other.SurfaceIndex;
        public override bool Equals(object obj) => obj is InstanceDataKey other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(PackerId, SurfaceIndex);
        public override string ToString() => $"{PackerId}[{SurfaceIndex}]";
    }
}