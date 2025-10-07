using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using UnityEngine;

namespace SpellBound.Core {
    /// <summary>
    /// ECS System to schedule Trigger events between ProxyEntityTags (acting as proxy for GameObjects),
    /// and any Entity it collides with. These entities should all have SpellBoundComponents,
    /// but the checks should be handle by collision layers, not by this system
    /// </summary>
    
    // The order is critical. 
    // Physics can run multiple times per frame and/or possibly none at all.
    // ProxyCollisionSystem Updates in the SimulationSystemGroup (not the Physics Group) to only update once per frame.
    // It fills the ECB which will playback after the Simulation and LateSimulation System Groups finish Updating.
    // And then ColliderRequestSystem runs early in the next frame, after the ebc has playedback the results of the job.
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(GoDeletionsCleanup))]
    public partial struct ProxyCollisionSystem : ISystem {
        // Threadsafe way to avoid processing the same collision multiple times
        private NativeParallelHashSet<Entity> _processedEntities;
        private NativeList<Entity> _pendingColliderRequests;
        
        public void OnCreate(ref SystemState state) {
            // These are neccesary for scheduling physics jobs.
            // EndSimulationEntityCommandBufferSystem gives access to the special ecb that playsback automatically.
            // These CANNOT be cached. They must be grabbed at the beginning of each frame.
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<SimulationSingleton>();
            
            // System turns on and off with the presence of ProxyEntityTags
            state.RequireForUpdate<ProxyEntityTag>();
            
            // Allocating Hashset
            _processedEntities = new NativeParallelHashSet<Entity>(capacity: 512, allocator: Allocator.Persistent);
            _pendingColliderRequests = new NativeList<Entity>(initialCapacity: 64, Allocator.Persistent);
        }

        public void OnDestroy(ref SystemState state) {
            if (_processedEntities.IsCreated)
                _processedEntities.Dispose();
            if (_pendingColliderRequests.IsCreated)
                _pendingColliderRequests.Dispose();
        }

        public void OnUpdate(ref SystemState state) {
            // Getting sim and ecb. Neccesary for scheduling physics jobs
            var sim = SystemAPI.GetSingleton<SimulationSingleton>();
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
            

            // Clearing HashSet. We want to reuse the memory, but none of the contents from previous frames.
            _processedEntities.Clear();
            _pendingColliderRequests.Clear();

            // Scheduling the Job. Note that sim is not passed into the job but into the handle.
            var job = new TriggerJob {
                ProxyTagLookup = SystemAPI.GetComponentLookup<ProxyEntityTag>(true),
                ProcessedEntities = _processedEntities.AsParallelWriter(),
                PendingColliderRequests = _pendingColliderRequests.AsParallelWriter()
            };

            // Scheduling like this means the jobhandle will force Complete after the Simulation and LateSimulationGroups run.
            // But the main thread can move onto other systems instead of waiting idle right here for the job to finish.
            var handle = job.Schedule(sim, state.Dependency);
            handle.Complete();

            foreach (var entity in _pendingColliderRequests)
            {
                if (!state.EntityManager.Exists(entity))
                    continue;
                if (state.EntityManager.HasComponent<PendingDestroyTag>(entity))
                    return;

                if (!state.EntityManager.HasComponent<ColliderRequest>(entity))
                    ecb.AddComponent(entity, new ColliderRequest { IsSpawnRequest = true });
                else
                    ecb.SetComponent(entity, new ColliderRequest { IsSpawnRequest = true });
            }
        }

        [BurstCompile]
        private struct TriggerJob : ITriggerEventsJob {
            [ReadOnly] public ComponentLookup<ProxyEntityTag> ProxyTagLookup;
            public NativeParallelHashSet<Entity>.ParallelWriter ProcessedEntities;
            public NativeList<Entity>.ParallelWriter PendingColliderRequests;

            public void Execute(TriggerEvent triggerEvent) {
                // Parsing the collision into what Entity is the Obstacle.
                var aIsProxy = ProxyTagLookup.HasComponent(triggerEvent.EntityA);
                var bIsProxy = ProxyTagLookup.HasComponent(triggerEvent.EntityB);

                if (!aIsProxy && !bIsProxy) {
                    return;
                }
                
                var obstacle = aIsProxy ? triggerEvent.EntityB : triggerEvent.EntityA;
                
                // Verifying we have not already processed this obstacle.
                if (!ProcessedEntities.Add(obstacle))
                    return;
                PendingColliderRequests.AddNoResize(obstacle);
            }
        }
    }
}