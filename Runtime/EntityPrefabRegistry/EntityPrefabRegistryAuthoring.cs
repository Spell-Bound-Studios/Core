// Copyright 2026 Spellbound Studio Inc.

using Unity.Entities;
using UnityEngine;

namespace Spellbound.Core {
    /// <summary>
    /// Combs the resource folder and populates buffer with entity prefabs and their guid.
    /// </summary>
    public sealed class EntityPrefabRegistryAuthoring : MonoBehaviour { }

    public sealed class PrefabRegistryBaker : Baker<EntityPrefabRegistryAuthoring> {
        public override void Bake(EntityPrefabRegistryAuthoring authoring) {
            var entity = GetEntity(TransformUsageFlags.None);

            AddComponent<EntityPrefabRegistryTag>(entity);

            var buffer = AddBuffer<EntityPrefabBufferElement>(entity);
            var presets = Resources.LoadAll<ObjectPreset>("");

            foreach (var preset in presets) {
                if (preset.bakePrefab == null) {
                    Debug.LogWarning($"Preset {preset.presetUid} has no bakePrefab assigned, skipping.");

                    continue;
                }

                buffer.Add(new EntityPrefabBufferElement {
                    Prefab = GetEntity(preset.bakePrefab, TransformUsageFlags.Renderable),
                    Guid = preset.presetUid
                });
            }
        }
    }
}