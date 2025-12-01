// Copyright 2025 Spellbound Studio Inc.

using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Spellbound.Core.ECS {
    /// <summary>
    /// ColliderPoolManager is an essential script for the ECS portion of Core.
    /// It is a singleton for creating and destroying temporary colliders on behalf of entities.
    /// TODO: Could benefit from ObjectPooling.
    /// </summary>
    public class ColliderPoolManager : MonoBehaviour {
        public static ColliderPoolManager Instance;
        private readonly Dictionary<Entity, GameObject> _activeColliders = new();
        [SerializeField] private int numberOfColliders;

        private void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);

                return;
            }

            Instance = this;
        }

        public bool TrySpawnCollider(
            Entity entity, float3 position, quaternion rotation, FixedString64Bytes presetUid) {
            if (_activeColliders.ContainsKey(entity)) return false;

            if (!presetUid.Value.ResolvePreset().TryGetModule(out EntityModule emodule)) return false;

            var go = emodule.proxyColliderObj;

            if (go == null) return false;

            _activeColliders[entity] = Instantiate(go, position, rotation);
            numberOfColliders = _activeColliders.Count;

            return true;
        }

        public void DespawnCollider(Entity entity) {
            if (_activeColliders.TryGetValue(entity, out var go)) {
                Destroy(go);
                _activeColliders.Remove(entity);
                numberOfColliders = _activeColliders.Count;
            }
        }
    }
}