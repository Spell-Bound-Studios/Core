// Copyright 2025 Spellbound Studio Inc.

using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Spellbound.Core {
    public partial struct PrefabRegistryBootstrapSystem : ISystem {
        public void OnUpdate(ref SystemState state) {
            var query = SystemAPI.QueryBuilder()
                    .WithAll<EntityPrefabRegistryTag>()
                    .Build();
            

            if (query.IsEmpty) {
                return;
            }

            if (!SingletonManager.TryGetSingletonInstance(out ObjectPresetDatabase db)) {
                return;
            }


            if (!SingletonManager.TryGetSingletonInstance(out EntityPrefabRegistry registry)) {
                return;
            }
                

            var registryEntity = query.GetSingletonEntity();
            var buffer = SystemAPI.GetBuffer<EntityPrefabBufferElement>(registryEntity);

            var guidToIndex = new Dictionary<string, int>();

            for (var i = 0; i < buffer.Length; i++) {
                var element = buffer[i];

                // Populate managed index lookup
                guidToIndex[element.Guid.ToString()] = i;

                // Populate Burst-compatible entity lookup
                registry.PrefabLookup[element.Guid] = element.Prefab;
            }

            db.RegisterEntityPrefabIndices(guidToIndex);

            state.Enabled = false;
        }
    }
}