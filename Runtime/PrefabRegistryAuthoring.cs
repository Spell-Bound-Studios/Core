// Copyright 2025 Spellbound Studio Inc.

using Unity.Entities;
using UnityEngine;

namespace Spellbound.Core {
    public sealed class PrefabRegistryAuthoring : MonoBehaviour { }

    public sealed class PrefabRegistryBaker : Baker<PrefabRegistryAuthoring> {
        public override void Bake(PrefabRegistryAuthoring authoring) {
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