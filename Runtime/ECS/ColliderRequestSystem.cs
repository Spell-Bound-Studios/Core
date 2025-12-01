// Copyright 2025 Spellbound Studio Inc.

using Unity.Entities;
using Unity.Transforms;

namespace Spellbound.Core.ECS {
    /// <summary>
    /// ColliderRequestSystem is an essential script for the ECS portion of Core. It is the responsible system for
    /// calling the ColliderPoolManager and processes ColliderRequests from the ProxyCollisionSystem.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup)), UpdateBefore(typeof(ProxyCollisionSystem))]
    public partial struct ColliderRequestSystem : ISystem {
        public void OnCreate(ref SystemState state) =>
                // System turns on and off with the presence of ColliderRequest
                state.RequireForUpdate<ColliderRequest>();

        public void OnUpdate(ref SystemState state) {
            // Allocating a new ecb. This is not for thread safety, it is for foreach safety.
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
            var entityManager = state.EntityManager;

            foreach (var (colliderRequest, transform, sbc, entity) in
                     SystemAPI.Query<RefRW<ColliderRequest>, RefRO<LocalTransform>, RefRO<SpellboundComponent>>()
                             .WithNone<PendingDestroyTag>()
                             .WithEntityAccess()) {
                // Timer counts down the interval before checking if the collider is still needed. 
                if (entityManager.HasComponent<TimerComponent>(entity))
                    continue;

                // IsSpawnRequest will be false if no ProxyCollider triggered it since the last time it's timer was set.
                if (!colliderRequest.ValueRW.IsSpawnRequest) {
                    ColliderPoolManager.Instance.DespawnCollider(entity);
                    ecb.RemoveComponent<ColliderRequest>(entity);
                }

                // IsSpawnRequest will be true if it's a new request or if a ProxyCollider is still triggering it.
                else {
                    ColliderPoolManager.Instance.TrySpawnCollider(
                        entity,
                        transform.ValueRO.Position,
                        transform.ValueRO.Rotation,
                        sbc.ValueRO.PresetUiD.ToString()
                    );

                    ecb.AddComponent<TimerComponent>(entity);

                    ecb.SetComponent(entity, new TimerComponent {
                        TimeRemaining = 1f
                    });

                    ecb.SetComponent(entity, new ColliderRequest {
                        IsSpawnRequest = false
                    });
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}