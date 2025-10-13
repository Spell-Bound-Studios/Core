// Copyright 2025 Spellbound Studio Inc.

using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using Helper = SpellBound.Core.SpellBoundStaticHelper;

namespace SpellBound.Core {
    [DisallowMultipleComponent]
    public class ProxyCollider : MonoBehaviour {
        private Entity _proxyEntity;
        private EntityManager _entityManager;
        private BlobAssetReference<Unity.Physics.Collider> _proxyCollider;
        private Vector3 _cachedPosition;

        private void Awake() => _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        private void OnEnable() => CreateProxyEntity();

        private void Update() {
            if (Vector3.Distance(transform.position, _cachedPosition) < 0.1f) return;

            _cachedPosition = transform.position;

            _entityManager.SetComponentData(_proxyEntity, new LocalTransform {
                Position = transform.position,
                Rotation = transform.rotation,
                Scale = 1f
            });
        }

        private void OnDisable() {
            if (_proxyCollider.IsCreated) _proxyCollider.Dispose();

            var world = World.DefaultGameObjectInjectionWorld;

            if (world == null || !world.IsCreated)
                return;

            var entityManager = world.EntityManager;

            if (!entityManager.Exists(_proxyEntity))
                return;

            entityManager.DestroyEntity(_proxyEntity);
            _proxyEntity = Entity.Null;
        }

        private void CreateProxyEntity() {
            // Define geometry
            var geom = new SphereGeometry { Center = float3.zero, Radius = 2f };

            var filter = new CollisionFilter {
                BelongsTo = (uint)Helper.EcsPhysicsLayers.ProxyCollider,
                CollidesWith = ~(uint)Helper.EcsPhysicsLayers.ProxyCollider,
                GroupIndex = 0
            };

            // Configure as Trigger:
            var mat = Unity.Physics.Material.Default;
            mat.CollisionResponse = CollisionResponsePolicy.RaiseTriggerEvents;

            // Create the blob with trigger behavior
            _proxyCollider = Unity.Physics.SphereCollider.Create(geom, filter, mat);

            _proxyEntity = _entityManager.CreateEntity();

            _entityManager.AddComponentData(_proxyEntity, new LocalTransform {
                Position = transform.position,
                Rotation = transform.rotation,
                Scale = 1f
            });

            _entityManager.AddComponentData(_proxyEntity, new PhysicsCollider {
                Value = _proxyCollider
            });
            _entityManager.AddComponent<ProxyEntityTag>(_proxyEntity);
            _entityManager.AddComponent<PhysicsWorldIndex>(_proxyEntity);

            _entityManager.AddComponentData(_proxyEntity, PhysicsMass.CreateKinematic(MassProperties.UnitSphere));
            _entityManager.AddComponentData(_proxyEntity, new PhysicsVelocity());
            _entityManager.AddComponent<Simulate>(_proxyEntity);
            _entityManager.SetName(_proxyEntity, "ProxyEntity");
        }
    }

    public struct ProxyEntityTag : IComponentData { }

    public struct ColliderRequest : IComponentData {
        public bool IsSpawnRequest;
    }

    public struct TimerComponent : IComponentData {
        public float TimeRemaining;
    }
}