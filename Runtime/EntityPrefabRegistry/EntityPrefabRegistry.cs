// Copyright 2026 Spellbound Studio Inc.

using Unity.Collections;
using Unity.Entities;

namespace Spellbound.Core {
    /// <summary>
    /// Access point for O(1) Entity Prefab Lookup by guid.
    /// </summary>
    public partial class EntityPrefabRegistry : SystemBase {
        private Entity _registryEntity;
        private bool _found;

        public NativeHashMap<FixedString64Bytes, Entity> PrefabLookup;

        protected override void OnStartRunning() {
            if (PrefabLookup.IsCreated)
                PrefabLookup.Dispose();
            PrefabLookup = new NativeHashMap<FixedString64Bytes, Entity>(64, Allocator.Persistent);
            SingletonManager.RegisterSingleton(this);
        }

        protected override void OnDestroy() {
            if (PrefabLookup.IsCreated)
                PrefabLookup.Dispose();
        }

        public Entity GetPrefab(int index) {
            if (!_found) {
                var query = SystemAPI.QueryBuilder()
                        .WithAll<EntityPrefabRegistryTag>()
                        .Build();

                if (query.IsEmpty)
                    return Entity.Null;

                _registryEntity = query.GetSingletonEntity();
                _found = true;
            }

            var buffer = SystemAPI.GetBuffer<EntityPrefabBufferElement>(_registryEntity);

            return buffer[index].Prefab;
        }

        protected override void OnUpdate() { }
    }
}