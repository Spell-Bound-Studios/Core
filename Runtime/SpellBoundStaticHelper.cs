using System;
using Unity.Entities;
using UnityEngine;

namespace SpellBound.Core {
    public static class SpellBoundStaticHelper {
        public const int ChunkSize = 128;
        
        [Flags]
        public enum EcsPhysicsLayers : uint {
            None = 0,
            Interactable = 1u << 18,
            ProxyCollider = 1u << 31,
            All = uint.MaxValue
        }
        public static event Action<Entity, GameObject> OnEntityInteraction;
        public static event Action<Entity> OnEntityDamage; //will include the amount and type of damage

        public static void InvokeOnEntityInteraction(Entity entity, GameObject interactor) {
            OnEntityInteraction?.Invoke(entity, interactor);
        }
        
        public static void InvokeOnEntityDamage(Entity entity) {
            OnEntityDamage?.Invoke(entity);
        }
    }
}

