// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using Spellbound.Core.Packing;
using UnityEngine;

namespace Spellbound.Core {
    [RequireComponent(typeof(Collider))]
    public class StaticEventSurface : MonoBehaviour, IEventSurface {
        [SerializeField, Tooltip("Decide your own surface index schema.")]
        private int surfaceIndex = -1;

        public Vector3 Position => transform.position;
        
        public GameObject GameObject => gameObject;
        
        public Transform Transform => transform;

        private IObjectParent _parent;
        private int _entityIndex;

        public ObjectPreset Preset { get; private set; }

        public void Initialize(IObjectParent objectParent, int entityIndex, string presetUid, Dictionary<InstanceDataKey, byte[]> dataSlots = null) {
            _parent = objectParent;
            _entityIndex = entityIndex;
            Preset = presetUid.ResolvePreset();

            // Check for children.
            var childSurfaces = GetComponentsInChildren<StaticEventSurface>(true);

            foreach (var childSurface in childSurfaces) {
                if (childSurface == this)
                    continue;

                childSurface.Initialize(_parent, _entityIndex, Preset.presetUid, dataSlots);
            }
        }

        public void DebugQueryPing() =>
                Debug.Log($"Pinging Event Surface for {Preset.name} " +
                          $"index {_entityIndex} " +
                          $"and surface index {surfaceIndex}");

        // Declare a THandler type at runtime that will pass in a pointer of that type to THAT types implementation.
        public void Dispatch<THandler>(Action<THandler, IObjectParent, int, string, int> invoke)
                where THandler : class {
            if (Preset == null)
                return;

            // If this event surface doesn't have children - early return.
            if (surfaceIndex < 0 || surfaceIndex >= Preset.surfaceModules.Count)
                return;

            // If it does have children loop through them and invoke.
            foreach (var module in Preset.surfaceModules[surfaceIndex].presetModules) {
                if (module is THandler handler)
                    invoke(handler, _parent, _entityIndex, Preset.presetUid, surfaceIndex);
            }
        }

        public void Receive(){
            AudioSource.PlayClipAtPoint(
                AudioClip.Create("beep", 4096, 1, 44100, false, data => {
                    for (int i = 0; i < data.Length; i++)
                        data[i] = Mathf.Sin(2 * Mathf.PI * 440f * i / 44100f);
                }),
                GameObject.transform.position
            );
        }
    }
}