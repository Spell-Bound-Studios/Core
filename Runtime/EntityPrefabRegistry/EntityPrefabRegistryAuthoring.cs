// Copyright 2025 Spellbound Studio Inc.

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
                if (preset.prefab == null) {
                    Debug.LogWarning($"Preset {preset.presetUid} has no prefab assigned, skipping.");
                    continue;
                }

                buffer.Add(new EntityPrefabBufferElement {
                    Prefab = GetEntity(preset.prefab, TransformUsageFlags.Renderable),
                    Guid = preset.presetUid
                });
            }
        }
    }
}