// Copyright 2026 Spellbound Studio Inc.

using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Spellbound.Core {
    [BurstCompile]
    public struct DynamicEventSurfaceAwakenJob : IJobParallelFor {
        [NativeDisableParallelForRestriction, ReadOnly] public NativeArray<float3> PovPositions;
        [NativeDisableParallelForRestriction, ReadOnly] public NativeList<ProximityCandidate> ProximityEntities;
        [NativeDisableParallelForRestriction] public NativeParallelHashSet<int>.ParallelWriter InstancesToAwaken;

        [BurstCompile]
        public void Execute(int index) {
            var instanceIndex = ProximityEntities[index].InstanceIndex;
            var position = ProximityEntities[index].Position;  
            var thresholdSq = ProximityEntities[index].Thresholds.x * ProximityEntities[index].Thresholds.x;

            foreach (var pov in PovPositions) {
                if (math.distancesq(pov, position) <= thresholdSq) {
                    InstancesToAwaken.Add(instanceIndex);
                    return;
                }
            }
        }
    }
}