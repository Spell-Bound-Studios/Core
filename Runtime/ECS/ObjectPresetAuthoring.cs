// Copyright 2026 Spellbound Studio Inc.

using Spellbound.Core.ECS;
using Unity.Entities;
using UnityEngine;

namespace Spellbound.Core {
    // Attach this MonoBehaviour to the prefab GameObject
    public class ObjectPresetAuthoring : MonoBehaviour {
        public ObjectPreset preset; // drag in the ScriptableObject
    }

    public class ObjectPresetBaker : Baker<ObjectPresetAuthoring> {
        public override void Bake(ObjectPresetAuthoring authoring) {
            var entity = GetEntity(TransformUsageFlags.Renderable);

            AddComponent(entity, new PresetUidComponent {
                Value = authoring.preset.presetUid
            });

            AddComponent(entity, new InstanceIndexComponent {
                Value = -1
            });
        }
    }
}