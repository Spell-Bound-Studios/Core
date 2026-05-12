// Copyright 2026 Spellbound Studio Inc.

using System.Collections.Generic;
using UnityEngine;

namespace Spellbound.Core {
    public interface IDynamicDataAccess {
        void SetConsumer(IObjectInstanceConsumer consumer);

        public bool HasInstance(int instanceIndex);
        
        void CreateRuntimeObject(string presetUid, Vector3 position, Vector3 rotation, int scale, List<(InstanceDataKey, byte[])> dataSlots = null);

        void Awaken(int instanceIndex);
        void Sleep(int instanceIndex, DynamicInstanceEntry entry, IEventSurface eventSurface);
    }
}