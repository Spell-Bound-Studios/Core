using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace SpellBound.Core
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(ProxyCollisionSystem))]
    public partial struct GoDeletionsCleanup: ISystem, ISystemStartStop
    {
        private Entity _goDeletionManager;
        
        [BurstCompile]
        public void OnStartRunning(ref SystemState state) {
            _goDeletionManager = SystemAPI.GetSingletonEntity<AddedGoDeletionBuffer>();
        }
        
        [BurstCompile]
        public void OnStopRunning(ref SystemState state) { }
        
        
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<AddedGoDeletionBuffer>();
        }
        
        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            var entityManager = state.EntityManager;
            
            var spawnBuffer = SystemAPI.GetBuffer<AddedGoDeletionBuffer>(_goDeletionManager);
            foreach (var goDeletion in spawnBuffer)
            {
                if (!entityManager.HasComponent<PendingDestroyTag>(goDeletion.Value)) {
                    ecb.AddComponent<PendingDestroyTag>(goDeletion.Value);
                    
                }
            }
            spawnBuffer.Clear();
            ecb.Playback(entityManager);
            ecb.Dispose();
        }

    }

}

