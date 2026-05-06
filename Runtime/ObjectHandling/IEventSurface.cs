// Copyright 2026 Spellbound Studio Inc.

using System;
using Spellbound.Core.Packing;
using UnityEngine;

namespace Spellbound.Core {
    public interface IEventSurface {
        
        GameObject GameObject { get; }
        
        Transform Transform { get; }
        
        ObjectPreset Preset { get; }
        
        void Initialize(IObjectParent parent, int entityIndex, string presetUid);

        void DebugQueryPing();
        
        void Dispatch<THandler>(Action<THandler, IObjectParent, int, string, int> invoke)
                where THandler : class;

        void Receive();
    }
}