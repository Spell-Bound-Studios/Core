// ProximityMath.cs
using Unity.Burst;
using Unity.Mathematics;

namespace Spellbound.Core {
    [BurstCompile]
    public static class ProximityMath {
        [BurstCompile]
        public static ProximityChange IsWithinChebyshevRange(in int3 a, in int3 b, in int2 threshold) {
            var chebyshevDist = math.cmax(math.abs(a - b));

            if (chebyshevDist < threshold.x) {
                return ProximityChange.Whitelist;
            }

            if (chebyshevDist > threshold.y) {
                return ProximityChange.Blacklist;
            }

            return ProximityChange.None;
        }
    }
}