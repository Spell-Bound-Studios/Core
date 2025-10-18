// Copyright 2025 Spellbound Studio Inc.

using Unity.Burst;
using Unity.Entities;

namespace Spellbound.Core {
    /// <summary>
    /// This system handles all the deletions of entities, based on their tag.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup)), UpdateAfter(typeof(ProxyCollisionSystem))]
    //[UpdateAfter(typeof(ChunkEcsCleanupSystem))]
    public partial struct DestroyTaggedEntitiesSystem : ISystem {
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<PendingDestroyTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

            foreach (var (tag, entity) in SystemAPI.Query<RefRO<PendingDestroyTag>>().WithEntityAccess())
                ecb.DestroyEntity(entity);
        }
    }
}