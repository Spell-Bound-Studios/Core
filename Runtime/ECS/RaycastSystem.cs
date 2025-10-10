// Copyright 2025 Spellbound Studio Inc.

using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;
using Object = UnityEngine.Object;
using Helper = SpellBound.Core.SpellBoundStaticHelper;

namespace SpellBound.Core {
    /// <summary>
    /// ECS/Gameobject observer system that performs raycasts and invokes out changes in the object preset.
    /// </summary>
    [UpdateInGroup(typeof(LateSimulationSystemGroup)), BurstCompile]
    public partial class RaycastSystem : SystemBase, ISystemStartStop {
        private Camera _playerCamera;
        private ObjectPreset _lastPreset;
        private bool _firedNullEvent;

        private const uint PhysicsLayer = 1u << 30;
        private const uint PhysicsLayerMask = (uint)Helper.EcsPhysicsLayers.Interactable;
        private const uint PhysicsRaycastDistance = 10;
        public static event Action<ObjectPreset> OnRaycastHitObjPreset;
        public static event Action<FixedString64Bytes> OnNgoIdHit;

        protected override void OnCreate() => RequireForUpdate<ProxyEntityTag>();

        [BurstCompile]
        public void OnStartRunning(ref SystemState state) => _firedNullEvent = false;

        [BurstCompile]
        public void OnStopRunning(ref SystemState state) { }

        protected override void OnUpdate() {
            if (_playerCamera == null) _playerCamera = Object.FindFirstObjectByType<Camera>();

            // Calculate camera current position and where you want the raycast to go.
            var start = _playerCamera.transform.position;
            var direction = _playerCamera.transform.forward;
            var end = start + direction * PhysicsRaycastDistance;

            var ecsHit = EcsRaycast(
                start,
                end,
                out var ecsOp,
                out var ecsDistance
            );

            var goHit = GoRaycast(
                start,
                direction,
                out var goOp,
                out var goDistance,
                out var ngoId
            );

            if (!ecsHit && goHit) {
                SendEvent(goOp);
                OnNgoIdHit?.Invoke(ngoId);

                return;
            }

            if (ecsHit && !goHit) {
                SendEvent(ecsOp);

                return;
            }

            if (ecsHit && goHit) {
                if (goDistance < ecsDistance) {
                    SendEvent(goOp);
                    OnNgoIdHit?.Invoke(ngoId);

                    return;
                }

                SendEvent(ecsOp);

                return;
            }

            if (_firedNullEvent)
                return;

            SendEvent(null, true);
        }

        private void SendEvent(ObjectPreset op, bool nullEvent = false) {
            if (_lastPreset == op)
                return;

            _lastPreset = op;
            OnRaycastHitObjPreset?.Invoke(_lastPreset);
            _firedNullEvent = nullEvent;
        }

        private bool EcsRaycast(float3 start, float3 end, out ObjectPreset op, out float distance) {
            op = null;
            distance = float.MaxValue;
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;

#if UNITY_EDITOR
            // Debugging
            Debug.DrawLine(start, end, Color.red, 0f);
#endif

            // Set up the raycast input.
            var ecsRay = new RaycastInput {
                Start = start,
                End = end,
                Filter = new CollisionFilter {
                    BelongsTo = PhysicsLayer,
                    CollidesWith = PhysicsLayerMask,
                    GroupIndex = 0
                }
            };

            // Keep an up-to-date reference of the CollisionWorld.
            var collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld.CollisionWorld;

            if (!collisionWorld.CastRay(ecsRay, out var hitEcs))
                return false;

            if (hitEcs.Entity == Entity.Null || !em.Exists(hitEcs.Entity))
                return false;

            if (!em.HasComponent<SpellBoundComponent>(hitEcs.Entity))
                return false;

            var sbbEcs = em.GetComponentData<SpellBoundComponent>(hitEcs.Entity);
            var preset = sbbEcs.PresetUiD.Value.ResolvePreset();

            distance = math.distance(start, hitEcs.Position);
            op = preset;

            return true;
        }

        private bool GoRaycast(
            float3 start, float3 direction, out ObjectPreset op, out float distance, out FixedString64Bytes ngoId) {
            op = null;
            distance = float.MaxValue;
            ngoId = default;

            if (!Physics.Raycast(start, direction, out var hitMono, PhysicsRaycastDistance))
                return false;

            if (!hitMono.collider.TryGetComponent<ISpellBoundBehaviour>(out var sbbMono))
                return false;

            op = sbbMono.GetObjectPreset();
            distance = math.distance(start, hitMono.point);

            return true;
        }
    }
}