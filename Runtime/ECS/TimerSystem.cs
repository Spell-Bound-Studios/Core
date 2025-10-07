using Unity.Burst;
using Unity.Entities;

namespace SpellBound.Core {
    
    /// <summary>
    /// ECS System for Managing all ECS Timers.
    /// Role of this System should never change or expand.
    /// Only changes could be the Update Order, and when the structural changes (RemoveComponent) should occur.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
    partial struct TimerSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<TimerComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // Allocating a new ecb. This is not for thread safety, it is for foreach safety.
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
            var entityManager = state.EntityManager;

            foreach (var (timerComponent, entity) in
                     SystemAPI.Query<RefRW<TimerComponent>>()
                         .WithNone<PendingDestroyTag>()
                         .WithEntityAccess()) {
                timerComponent.ValueRW.TimeRemaining -= SystemAPI.Time.DeltaTime;
                if (timerComponent.ValueRW.TimeRemaining <= 0) {
                    ecb.RemoveComponent<TimerComponent>(entity);
                }
            }
            ecb.Playback(entityManager);
            ecb.Dispose();
        }
    }
}

