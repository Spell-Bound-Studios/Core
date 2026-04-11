// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Spellbound.Core.ECS;
using Spellbound.Core.Packing;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Spellbound.Core {
    /// <summary>
    /// Poco1
    /// Poco to assist management of object data by the parent.
    /// </summary>
    public class ObjectParent : IDisposable {
        private readonly IObjectParent _implementer;
        private readonly Transform _transform;

        private Action<LocalTransform, int, ObjectPreset> _surfaceSpawnAction;
        public IObjectDataAccess DataAccess { get; }

        private readonly Dictionary<int, EventSurface> _eventSurfaces = new();
        private readonly Dictionary<int, Entity> _entities = new();

        public void Dispose() {
            DataAccess.OnInstanceRemoved -= HandleInstanceRemoved;
            DataAccess.OnInstanceCreated -= HandleInstanceAdded;

            var world = World.DefaultGameObjectInjectionWorld;
            if (world is not { IsCreated: true })
                return;

            var em = world.EntityManager;
            using var entities = new NativeArray<Entity>(_entities.Values.ToArray(), Allocator.Temp);
            em.DestroyEntity(entities);
        }

        public ObjectParent(
            IObjectParent implementer, Transform transform, IObjectDataAccess dataAccess, Vector3Int parentId,
            Action<LocalTransform, int, ObjectPreset> surfaceSpawnAction = null) {
            _implementer = implementer;
            _transform = transform;
            DataAccess = dataAccess;
            _surfaceSpawnAction = surfaceSpawnAction ?? SpawnSurface;
            DataAccess.OnInstanceRemoved += HandleInstanceRemoved;
            DataAccess.OnInstanceCreated += HandleInstanceAdded;
        }

        public void CreateNewInstance(ObjectPreset preset, Vector3 position, Vector3 rotation, int scale) =>
                DataAccess.CreateInstance(preset.presetUid, position, rotation, scale);

        public void ActivateObjects(NativeList<PristineGoData> objects) {
            if (!objects.IsCreated || objects.Length == 0)
                return;

            var idx = 0;

            var em = World.DefaultGameObjectInjectionWorld.EntityManager;

            foreach (var data in objects) {
                if (DataAccess.IsDeleted(idx)) {
                    idx++;

                    continue;
                }

                var entity = em.Instantiate(data.entityPrefab);

                em.SetComponentData(entity, LocalTransform.FromPositionRotationScale(
                    data.position,
                    quaternion.Euler(data.rotation),
                    data.scale.x
                ));

                em.SetComponentData(entity, new InstanceIndexComponent {
                    Value = idx
                });
                _entities[idx] = entity;
                idx++;
            }

            DataAccess.SetNextInstanceIndex(idx);
        }

        private void HandleInstanceAdded(
            int instanceIndex, string presetUid, TransformData transformData) {
            if (!presetUid.TryGetEntityPrefab(out var prefab)) return;

            var em = World.DefaultGameObjectInjectionWorld.EntityManager;

            var entity = em.Instantiate(prefab);

            em.SetComponentData(entity, LocalTransform.FromPositionRotationScale(
                transformData.Position,
                quaternion.Euler(transformData.Rotation),
                transformData.Scale
            ));

            em.SetComponentData(entity, new InstanceIndexComponent {
                Value = instanceIndex
            });
            _entities[instanceIndex] = entity;
        }

        public bool TryReadData<T>(int instanceIndex, string presetUid, int eventSurfaceIndex, out T result)
                where T : IPacker, new() {
            if (DataAccess.TryRead<T>(instanceIndex, eventSurfaceIndex, out var data)) {
                result = data;
                Debug.Log($"result is {result}");

                return true;
            }

            result = default;

            return false;
        }

        public bool TryWriteData<T>(int instanceIndex, string presetUid, int eventSurfaceIndex, T newData)
                where T : IPacker, new() {
            DataAccess.Write(instanceIndex, presetUid, eventSurfaceIndex, newData);

            return true;
        }

        public bool TryTransformData<T>(
            int instanceIndex, string presetUid, int eventSurfaceIndex, T delta) where T : IQuantitativeData, new() {
            Debug.Log($"Calling TryTransformData on instanceIndex {instanceIndex}, delta {delta}");

            DataAccess.Delta(instanceIndex, presetUid, eventSurfaceIndex, delta);

            return true;
        }

        public async Task<bool> TryDeleteData(int instanceIndex) => await DataAccess.TryDeleteInstance(instanceIndex);

        private void HandleInstanceRemoved(int instanceIndex) {
            DeleteEntity(instanceIndex);
            DeleteEventSurface(instanceIndex);
        }

        private void DeleteEventSurface(int instanceIndex) {
            if (!_eventSurfaces.TryGetValue(instanceIndex, out var surface)) return;

            UnityEngine.Object.Destroy(surface.gameObject);
            _eventSurfaces.Remove(instanceIndex);
        }

        private void DeleteEntity(int instanceIndex) {
            if (!_entities.TryGetValue(instanceIndex, out var entity)) return;

            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            em.DestroyEntity(entity);
            _entities.Remove(instanceIndex);
        }

        public void SpawnSurface(LocalTransform transform, int instanceIndex, ObjectPreset preset) {
            var eventSurface = UnityEngine.Object.Instantiate(
                preset.eventSurfacePrefab,
                transform.Position,
                transform.Rotation,
                _transform
            );
            eventSurface.gameObject.name = $"{preset.name} Event Surface {instanceIndex}";
            eventSurface.transform.localScale = transform.Scale * Vector3.one;
            eventSurface.Initialize(_implementer, instanceIndex, preset.presetUid);
            _eventSurfaces[instanceIndex] = eventSurface;
        }

        private void DespawnSurface(int instanceIndex) {
            if (!_eventSurfaces.TryGetValue(instanceIndex, out var proxy))
                return;

            UnityEngine.Object.Destroy(proxy.gameObject);
            _eventSurfaces.Remove(instanceIndex);
        }

        public void EntityDistanceQuery(Vector3 playerPosition) {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;

            foreach (var entity in _entities.Values) {
                var transform = em.GetComponentData<LocalTransform>(entity);
                var presetUid = em.GetComponentData<PresetUidComponent>(entity).Value;
                var instanceIndex = em.GetComponentData<InstanceIndexComponent>(entity).Value;
                var preset = presetUid.Value.ResolvePreset();

                var distance = Vector3.Distance(playerPosition, transform.Position);
                var hasProxy = _eventSurfaces.ContainsKey(instanceIndex);

                if (distance < preset.interactionDistance && !hasProxy)
                    SpawnSurface(transform, instanceIndex, preset);
                else if (distance > preset.interactionDistance + 10f && hasProxy)
                    DespawnSurface(instanceIndex);
            }
        }
    }
}