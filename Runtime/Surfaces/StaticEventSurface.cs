// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using Spellbound.Core.Logging;
using Spellbound.Core.ObjectData;
using Spellbound.Core.ObjectHandling;
using Spellbound.Core.Objects;
using Spellbound.Core.Packing;
using Spellbound.Core.PresetContracts;
using UnityEngine;

namespace Spellbound.Core.Surfaces {
    [RequireComponent(typeof(Collider))]
    public class StaticEventSurface : MonoBehaviour, IEventSurface {
        [SerializeField, Tooltip("Decide your own surface index schema.")]
        private int surfaceIndex = -1;

        public Vector3 Position => transform.position;

        public GameObject GameObject => gameObject;

        public Transform Transform => transform;

        private IObjectParent _parent;
        private int _entityIndex;

        private Dictionary<int, IEventSurface> _childEventSurfaces = new();

        public ObjectPreset Preset { get; private set; }

        public int Initialize(
            IObjectParent objectParent, int entityIndex, string presetUid,
            Dictionary<InstanceDataKey, byte[]> dataSlots = null) {
            _parent = objectParent;
            _entityIndex = entityIndex;
            Preset = presetUid.ResolvePreset();

            var childSurfaces = GetComponentsInChildren<StaticEventSurface>(true);

            foreach (var childSurface in childSurfaces) {
                if (childSurface == this)
                    continue;

                var childSurfaceIndex = childSurface.Initialize(_parent, _entityIndex, Preset.presetUid, dataSlots);

                if (!_childEventSurfaces.TryAdd(childSurfaceIndex, childSurface))
                    Log.Error($"Duplicate surfaceIndex {childSurfaceIndex} on {childSurface.gameObject.name}");
            }

            return surfaceIndex;
        }

        public void DebugQueryPing() =>
                Debug.Log($"Pinging Event Surface for {Preset.name} " +
                          $"index {_entityIndex} " +
                          $"and surface index {surfaceIndex}");

        // Declare a THandler type at runtime that will pass in a pointer of that type to THAT types implementation.
        public bool Dispatch<TContext>(TContext dispatch) where TContext : IPackerDispatch {
            if (Preset == null)
                return false;

            // If this event surface doesn't have children - early return.
            if (surfaceIndex < 0 || surfaceIndex >= Preset.surfaceModules.Count)
                return false;

            // If it does have children loop through them and invoke.
            foreach (var module in Preset.surfaceModules[surfaceIndex].presetModules) {
                if (module is IDispatch<TContext> handler)
                    handler.OnDispatch(dispatch, this, _parent, _entityIndex);
            }
            return false;
        }

        public event Action OnChanged;

        public void AlertChanged() => OnChanged?.Invoke();

        public bool TryGetEventSurfaceByIndex(int desiredSurfaceIndex, out IEventSurface surface) {
            if (desiredSurfaceIndex == surfaceIndex) {
                surface = this;

                return true;
            }

            return _childEventSurfaces.TryGetValue(desiredSurfaceIndex, out surface);
        }
        
        //TODO
        public bool TryRead<T>(out T data) where T : IPackerObjectData, new() => throw new NotImplementedException();

        public bool TryWrite<T>(T data, byte contextIn) where T : IPackerObjectData, new() => throw new NotImplementedException();

        public bool TryDestroy() => throw new NotImplementedException();
    }
}