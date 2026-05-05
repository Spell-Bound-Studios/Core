﻿// Copyright 2026 Spellbound Studio Inc.

using Spellbound.Core.ECS;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace Spellbound.Core {
    [BurstCompile]
    public struct StaticEventSurfaceProximityJob : IJobParallelFor {
        [NativeDisableParallelForRestriction, ReadOnly] public NativeArray<float3> PovPositions;

        [NativeDisableParallelForRestriction, ReadOnly] public NativeArray<LocalTransform> Transforms;
        
        [NativeDisableParallelForRestriction, ReadOnly] public NativeArray<StaticProximityObjectComponent> Thresholds;

        [NativeDisableParallelForRestriction, ReadOnly] public NativeArray<InstanceIndexComponent> InstanceIndices;
        
        
        [NativeDisableParallelForRestriction, ReadOnly] public NativeHashSet<int> ExistingEventSurfaces;
        
        [NativeDisableParallelForRestriction] public NativeParallelHashSet<int>.ParallelWriter InstancesToAwaken;
        [NativeDisableParallelForRestriction] public NativeParallelHashSet<int>.ParallelWriter InstancesToSleep;

        [BurstCompile]
        public void Execute(int index) {
            var instanceIndex = InstanceIndices[index].Value;
            var position = Transforms[index].Position;
            var thresholds = Thresholds[index].Value;
            
            var canSleep = true;

            foreach (var povPosition in PovPositions) {
                switch (ProximityMath.IsWithinActivationRange(povPosition, position, thresholds)) {
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
                InstancesToSleep.Add(index);
            }
        }
    }
}