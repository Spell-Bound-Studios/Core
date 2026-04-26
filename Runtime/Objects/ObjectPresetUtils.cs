// Copyright 2026 Spellbound Studio Inc.

using Unity.Entities;
using UnityEngine;

namespace Spellbound.Core {
    public static class ObjectPresetUtils {
        public static ObjectPreset ResolvePreset(this string uid) =>
                !string.IsNullOrEmpty(uid) &&
                SingletonManager.TryGetSingletonInstance(out ObjectPresetDatabase db) &&
                db.TryGetPreset(uid, out var preset)
                        ? preset
                        : null;

        public static bool TryGetEntityPrefab(this string uid, out Entity entity) {
            if (!SingletonManager.TryGetSingletonInstance(out EntityPrefabRegistry registry)) {
                Debug.LogWarning("EntityPrefabRegistry not found");
                entity = Entity.Null;

                return false;
            }

            if (!registry.PrefabLookup.TryGetValue(uid, out entity)) {
                Debug.LogWarning($"No entity bakePrefab found for preset {uid.ResolvePreset()}");

                return false;
            }

            return true;
        }
    }
}