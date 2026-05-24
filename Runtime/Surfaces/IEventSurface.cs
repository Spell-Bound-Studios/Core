// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using Spellbound.Core.ObjectData;
using Spellbound.Core.ObjectHandling;
using Spellbound.Core.Objects;
using UnityEngine;

namespace Spellbound.Core.Surfaces {
    public interface IEventSurface : IDispatchSurface {
        GameObject GameObject { get; }

        Transform Transform { get; }

        ObjectPreset Preset { get; }

        int Initialize(
            IObjectParent objectParent, int entityIndex, string presetUid,
            Dictionary<InstanceDataKey, byte[]> dataSlots = null);

        void DebugQueryPing();
        
        public event Action OnChanged;

        void AlertChanged();

        bool TryGetEventSurfaceByIndex(int desiredSurfaceIndex, out IEventSurface surface);
    }
}