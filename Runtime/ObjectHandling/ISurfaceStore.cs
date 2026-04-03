// Copyright 2026 Spellbound Studio Inc.

using System;
using Unity.Transforms;
using UnityEngine;


namespace Spellbound.Core {
    public interface ISurfaceStore {
        event Action<LocalTransform, int, string> OnRequestInstantiateSurface;
        
        public Transform ParentTransform { get; set; }
        void SpawnSurface(LocalTransform localTransform, int instanceIndex, string presetUid);
        
        void RegisterSurface(int instanceIndex, EventSurface surface);
        void DespawnSurface(int instanceIndex);
        
        public bool TryGetSurface(int instanceIndex, out EventSurface surface);
        
        void UnregisterSurface(int instanceIndex);
        bool TryGetTransform(int instanceIndex, out Transform transform);
    }
}