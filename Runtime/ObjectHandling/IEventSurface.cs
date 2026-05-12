// Copyright 2026 Spellbound Studio Inc.

using System;
using Spellbound.Core.Packing;
using UnityEngine;

namespace Spellbound.Core {
    public interface IEventSurface {
        
        GameObject GameObject { get; }
        
        Transform Transform { get; }
        
        ObjectPreset Preset { get; }
        
        int Initialize(IObjectParent parent, int entityIndex, string presetUid);

        void DebugQueryPing();
        
        void Dispatch<TContext>(TContext context)
                where TContext : struct;

        public event Action OnChanged;

        void AlertChanged();

        bool TryGetEventSurfaceByIndex(int desiredSurfaceIndex, out IEventSurface surface);


    }
}