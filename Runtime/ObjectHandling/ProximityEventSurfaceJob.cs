// Copyright 2026 Spellbound Studio Inc.

using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Spellbound.Core {
    [BurstCompile]
    public struct ProximityEventSurfaceJob : IJobParallelFor {
        [NativeDisableParallelForRestriction, ReadOnly] public NativeArray<int3> PovPositions;
        
        [NativeDisableParallelForRestriction, ReadOnly] public NativeList<ProximityEntity> ProximityEntities;
        
        [NativeDisableParallelForRestriction, ReadOnly] public NativeHashSet<int> ExistingEventSurfaces;
        
        [NativeDisableParallelForRestriction] public NativeParallelHashSet<int>.ParallelWriter InstancesToAwaken;
        [NativeDisableParallelForRestriction] public NativeParallelHashSet<int>.ParallelWriter InstancesToSleep;

        [BurstCompile]
        public void Execute(int index) {
            var instanceIndex = ProximityEntities[index].InstanceIndex;
            var position = ProximityEntities[index].Position;
            var thresholds = ProximityEntities[index].Thresholds;
            var isDynamic = ProximityEntities[index].IsDynamic;
            
            var canSleep = true;

            foreach (var playerCoord in PovPositions) {
                switch (ProximityMath.IsWithinChebyshevRange(playerCoord, position, thresholds)) {
                    case ProximityChange.Whitelist:
                        if (!ExistingEventSurfaces.Contains(instanceIndex)) {
                            InstancesToAwaken.Add(index);
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