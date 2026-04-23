// Copyright 2026 Spellbound Studio Inc.

using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Spellbound.Core {
    [BurstCompile]
    public struct StaticEventSurfaceProximityJob : IJobParallelFor {
        [NativeDisableParallelForRestriction, ReadOnly] public NativeArray<float3> PovPositions;
        
        [NativeDisableParallelForRestriction, ReadOnly] public NativeList<ProximityCandidate> ProximityEntities;
        
        [NativeDisableParallelForRestriction, ReadOnly] public NativeHashSet<int> ExistingEventSurfaces;
        
        [NativeDisableParallelForRestriction] public NativeParallelHashSet<int>.ParallelWriter InstancesToAwaken;
        [NativeDisableParallelForRestriction] public NativeParallelHashSet<int>.ParallelWriter InstancesToSleep;

        [BurstCompile]
        public void Execute(int index) {
            var instanceIndex = ProximityEntities[index].InstanceIndex;
            var position = ProximityEntities[index].Position;
            var thresholds = ProximityEntities[index].Thresholds;
            
            var canSleep = true;

            foreach (var povPosition in PovPositions) {
                switch (ProximityMath.IsWithinActivationRange(povPosition, position, thresholds)) {
                    case ProximityChange.Whitelist:
                        if (!ExistingEventSurfaces.Contains(instanceIndex)) {
                            InstancesToAwaken.Add(instanceIndex);
                        }

                        return;
                    case ProximityChange.None:
                        canSleep = false;

                        break;
                    
                    case ProximityChange.Blacklist:
                        // Too far — leave canSleep = true, keep checking other povs
                        break;
                }
            }
            if (canSleep && ExistingEventSurfaces.Contains(instanceIndex)) {
                InstancesToSleep.Add(instanceIndex);
            }
        }
    }
}