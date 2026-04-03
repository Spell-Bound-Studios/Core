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
using Object = System.Object;

namespace Spellbound.Core {
    /// <summary>
    /// Poco1
    /// Poco to assist management of object data by the parent.
    /// </summary>
    public class ObjectParent : IDisposable{
        private IObjectParent _implementer;
        private Transform _transform;
        private readonly IObjectDataStore _dataStore;
        public IEventSurfaceStore EventSurfaces;
        
        public IObjectDataStore DataStore => _dataStore;
        private Dictionary<int, Entity> _entities = new();
        public Dictionary<int, Entity> Entities => _entities;

        private Vector3 _lastPlayerPosition;

        public void Dispose() {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
    
            using var entities = new NativeArray<Entity>(_entities.Values.ToArray(), Allocator.Temp);
            em.DestroyEntity(entities);
            
            _dataStore.OnInstanceRemoved -= HandleInstanceRemoved;
            _dataStore.OnInstanceCreated -= HandleInstanceAdded;
        }
        

        public ObjectParent(IObjectParent implementer, Transform transform, IObjectDataStore dataStore, IEventSurfaceStore eventSurfacesStore, Vector3Int parentId) {
            _implementer = implementer;
            _transform = transform;
            _dataStore = dataStore;
            EventSurfaces = eventSurfacesStore;
            _dataStore.OnInstanceRemoved += HandleInstanceRemoved;
            _dataStore.OnInstanceCreated += HandleInstanceAdded;
        }

        public void CreateNewInstance(ObjectPreset preset, Vector3 position, Vector3 rotation, int scale) {
            Debug.Log("ObjectParent creating the instance");
            _dataStore.CreateInstance(preset.presetUid, position, rotation, scale);
        }

        public void ActivateObjects(NativeList<PristineGoData> objects) {
            if (!objects.IsCreated || objects.Length == 0) 
                return;

            var idx = 0;
            
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            
            for (int i = 0; i < objects.Length; i++) {
                var data = objects[i];
                
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
            _dataStore.NextInstanceIndex = idx;
        }

        private void HandleInstanceAdded(
            int instanceIndex, string presetUid, Vector3 position, Vector3 rotation, int scale) {
            if (!presetUid.TryGetEntityPrefab(out var prefab)) {
                return;
            }
            
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            
            var entity = em.Instantiate(prefab);
                
            em.SetComponentData(entity, LocalTransform.FromPositionRotationScale(
                position,
                quaternion.Euler(rotation),
                scale 
            ));
            em.SetComponentData(entity, new InstanceIndexComponent {
                Value = instanceIndex
            });
            _entities[instanceIndex] = entity;
            OneEntityDistanceQuery(entity);
        }
        

        public bool TryReadData<T>(int instanceIndex, string presetUid, int eventSurfaceIndex, out T result) where T : IPacker, new() {
            if (_dataStore.TryRead<T>(instanceIndex, eventSurfaceIndex, out var data)) {
                result = data;
                Debug.Log($"result is {result}");
                return true;
            }

            result = default;
            return false;
        }
        
        public bool TryWriteData<T>(int instanceIndex, string presetUid, int eventSurfaceIndex, T newData) 
                where T : IPacker, new() {


            if (!_dataStore.HasInstance(instanceIndex))
                _dataStore.StoreInstance(instanceIndex, presetUid);
            
            _dataStore.Write(instanceIndex, presetUid, eventSurfaceIndex, newData);
            return true;
        }

        public bool TryTransformData<T>(
            int instanceIndex, string presetUid, int eventSurfaceIndex, T delta) where T : IQuantitativeData, new() {
           
            
            Debug.Log($"Calling TryTransformData on instanceIndex {instanceIndex}, delta {delta}");
            
            
            _dataStore.Delta(instanceIndex, presetUid, eventSurfaceIndex, delta);
            
            return true;
        }
        
        public async Task<bool> TryDeleteData(int instanceIndex) => await _dataStore.DeleteInstance(instanceIndex);

        private void HandleInstanceRemoved(int instanceIndex) {
            DeleteEntity(instanceIndex);
            DeleteEventSurface(instanceIndex);
        }

        private void DeleteEventSurface(int instanceIndex) {
            if (!EventSurfaces.TryGetEventSurface(instanceIndex, out var surface)) {
                return;
            }
            UnityEngine.Object.Destroy(surface.gameObject);
            EventSurfaces.Unregister(instanceIndex);
        }

        private void DeleteEntity(int instanceIndex) {
            if (!_entities.TryGetValue(instanceIndex, out var entity)) {
                return;
            }

            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            em.DestroyEntity(entity);
            _entities.Remove(instanceIndex);
        }
        
        public void FullEntityDistanceQuery(Vector3 playerPosition) {
            _lastPlayerPosition = playerPosition;
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            
            foreach(var entity in _entities.Values) {
                
                var transform = em.GetComponentData<LocalTransform>(entity);
                var presetUid = em.GetComponentData<PresetUidComponent>(entity).Value;
                var instanceIndex = em.GetComponentData<InstanceIndexComponent>(entity).Value;
                var preset = presetUid.Value.ResolvePreset();
        
                float distance = Vector3.Distance(playerPosition, transform.Position);
                bool hasProxy = EventSurfaces.HasIndex(instanceIndex);

                if (distance < preset.interactionDistance && !hasProxy)
                    _implementer.SpawnSurface(transform, instanceIndex, preset);
                else if (distance > preset.interactionDistance + 10f && hasProxy)
                    _implementer.DespawnSurface(instanceIndex);
            }
        }

        public void OneEntityDistanceQuery(Entity entity) {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            var transform = em.GetComponentData<LocalTransform>(entity);
            var presetUid = em.GetComponentData<PresetUidComponent>(entity).Value;
            var instanceIndex = em.GetComponentData<InstanceIndexComponent>(entity).Value;
            var preset = presetUid.Value.ResolvePreset();
        
            float distance = Vector3.Distance(_lastPlayerPosition, transform.Position);
            bool hasProxy = EventSurfaces.HasIndex(instanceIndex);

            if (distance < preset.interactionDistance && !hasProxy)
                _implementer.SpawnSurface(transform, instanceIndex, preset);
            else if (distance > preset.interactionDistance + 10f && hasProxy)
                _implementer.DespawnSurface(instanceIndex);
        }

        public void SpawnSurface(LocalTransform localTransform, int instanceIndex, ObjectPreset preset) {
            var es = UnityEngine.Object.Instantiate(
                preset.eventSurfacePrefab,
                localTransform.Position,
                localTransform.Rotation,
                _transform
            );
            es.transform.localScale = localTransform.Scale * Vector3.one;
            es.transform.name = $"{preset.name}_eventSurface_{instanceIndex}";
            EventSurfaces.Register(instanceIndex, es);
            es.Initialize(instanceIndex, preset);
        }

        public void DespawnSurface(int instanceIndex) {
            if (EventSurfaces.TryGetEventSurface(instanceIndex, out var surface)) {
                EventSurfaces.Unregister(instanceIndex);
                UnityEngine.Object.Destroy(surface.gameObject);
            }
        }
        
        private static string GetPackerId<T>() {
            var id = PackerIdCache<T>.Id;

            if (id == null)
                Debug.LogWarning($"[ObjectParent] {typeof(T).Name} is missing a PackerIdAttribute.");

            return id;
        }
    }
}