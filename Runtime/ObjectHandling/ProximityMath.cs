// Copyright 2026 Spellbound Studio Inc.

using Unity.Burst;
using Unity.Mathematics;

namespace Spellbound.Core {
    [BurstCompile]
    public static class ProximityMath {
        [BurstCompile]
        public static ProximityChange IsWithinActivationRange(in float3 a, in float3 b, in float2 thresholds) {
            var distSq = math.distancesq(a, b);

            if (distSq < thresholds.x * thresholds.x)
                return ProximityChange.Whitelist;

            if (distSq > thresholds.y * thresholds.y)
                return ProximityChange.Blacklist;

            return ProximityChange.None;
        }
    }
}