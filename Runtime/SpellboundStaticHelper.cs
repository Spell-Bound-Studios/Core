// Copyright 2025 Spellbound Studio Inc.

using System;
using Unity.Entities;
using UnityEngine;

namespace Spellbound.Core {
    /// <summary>
    /// WIP Helper methods. Likely won't make it to main for the time being.
    /// </summary>
    public static class SpellboundStaticHelper {

        [Flags]
        public enum EcsPhysicsLayers : uint {
            None = 0,
            Interactable = 1u << 18,
            ProxyCollider = 1u << 31,
            All = uint.MaxValue
        }

        public static event Action<Entity, GameObject> OnEntityInteraction;
        public static event Action<Entity> OnEntityDamage; //will include the amount and type of damage

        public static void InvokeOnEntityInteraction(Entity entity, GameObject interactor) =>
                OnEntityInteraction?.Invoke(entity, interactor);

        public static void InvokeOnEntityDamage(Entity entity) => OnEntityDamage?.Invoke(entity);
    }
}