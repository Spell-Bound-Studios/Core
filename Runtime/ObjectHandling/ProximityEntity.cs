// Copyright 2026 Spellbound Studio Inc.

using Unity.Mathematics;

namespace Spellbound.Core {
    public struct ProximityEntity {
        public int3 Position;
        public int2 Thresholds;
        public int InstanceIndex;
        public bool IsDynamic;
    }
}