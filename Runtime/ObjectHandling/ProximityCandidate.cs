// Copyright 2026 Spellbound Studio Inc.

using Unity.Mathematics;

namespace Spellbound.Core {
    public struct ProximityCandidate {
        public float3 Position;
        public float2 Thresholds;
        public int InstanceIndex;
    }
}