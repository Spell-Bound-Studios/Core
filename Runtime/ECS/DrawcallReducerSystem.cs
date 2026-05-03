// Copyright 2026 Spellbound Studio Inc.

using Spellbound.Core.ECS;
using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;

namespace Spellbound.Core {
    public partial struct DrawcallReducerSystem : ISystem  {
        
        private EntityQuery _query;
        private int _currentIndex;
        public void OnCreate(ref SystemState state) {
            _query = SystemAPI.QueryBuilder()
                    .WithAll<PresetUidComponent>()
                    .WithAllChunkComponent<EntitiesGraphicsChunkInfo>()
                    .Build();
            _currentIndex = 0;
        }

        public void OnUpdate(ref SystemState state) {
            var chunks = _query.ToArchetypeChunkArray(Allocator.Temp);

            if (chunks.Length == 0) {
                chunks.Dispose();
                return;
            }

            var sharedHandle = state.GetSharedComponentTypeHandle<PresetUidComponent>();

            var uniqueValues = new NativeList<PresetUidComponent>(Allocator.Temp);
            foreach (var chunk in chunks) {
                var val = chunk.GetSharedComponent(sharedHandle);
                bool found = false;
                foreach (var existing in uniqueValues) {
                    if (existing.Equals(val)) { found = true; break; }
                }
                if (!found) uniqueValues.Add(val);
            }

            if (uniqueValues.Length == 0) {
                chunks.Dispose();
                uniqueValues.Dispose();
                return;
            }

            _currentIndex = _currentIndex % uniqueValues.Length;
            var presetUid = uniqueValues[_currentIndex];
            _currentIndex++;
            

            foreach (var chunk in chunks) {
                if (!chunk.GetSharedComponent(sharedHandle).Equals(presetUid)) continue;
                state.EntityManager.SetChunkComponentData(chunk, default(EntitiesGraphicsChunkInfo));
            }

            chunks.Dispose();
            uniqueValues.Dispose();
        }
    }
}