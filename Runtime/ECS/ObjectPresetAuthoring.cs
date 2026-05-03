// Copyright 2026 Spellbound Studio Inc.

using Spellbound.Core.ECS;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Spellbound.Core {
    // Attach this MonoBehaviour to the prefab GameObject
    public class ObjectPresetAuthoring : MonoBehaviour {
        public ObjectPreset preset; // drag in the ScriptableObject
    }

    public class ObjectPresetBaker : Baker<ObjectPresetAuthoring> {
        public override void Bake(ObjectPresetAuthoring authoring) {
            if (authoring.preset == null) {
                Debug.LogError($"[ObjectPresetBaker] Missing ObjectPreset on '{authoring.gameObject.name}'!",
                    authoring);

                return;
            }

            var entity = GetEntity(TransformUsageFlags.Renderable);

            AddSharedComponentManaged(entity, new PresetUidComponent {
                Value = authoring.preset.presetUid
            });

            AddComponent(entity, new StaticProximityObjectComponent {
                Value = new float2(authoring.preset.interactionDistance)
            });

            AddComponent(entity, new InstanceIndexComponent {
                Value = -1
            });

            AddSharedComponent(entity, new ChunkParentComponent());
        }
    }
}