// Copyright 2026 Spellbound Studio Inc.

using Spellbound.Core.EntityPrefabs;
using Spellbound.Core.Logging;
using Spellbound.Core.Tooling;
using Unity.Entities;

namespace Spellbound.Core.Objects {
    public static class ObjectPresetUtils {
        public static ObjectPreset ResolvePreset(this string uid) =>
                !string.IsNullOrEmpty(uid) &&
                SingletonManager.TryGetSingletonInstance(out ObjectPresetDatabase db) &&
                db.TryGetPreset(uid, out var preset)
                        ? preset
                        : null;

        public static bool TryGetEntityPrefab(this string uid, out Entity entity) {
            if (!SingletonManager.TryGetSingletonInstance(out EntityPrefabRegistry registry)) {
                Log.Warn("EntityPrefabRegistry not found");
                entity = Entity.Null;

                return false;
            }

            if (registry.PrefabLookup.TryGetValue(uid, out entity))
                return true;

            Log.Warn($"No entity bakePrefab found for preset {uid.ResolvePreset()}");

            return false;
        }
    }
}