// Copyright 2026 Spellbound Studio Inc.

using System;

namespace Spellbound.Core {
    public interface IObjectDataStore {
        event Action<int> OnInstanceRemoved;
        event Action<int> OnInstanceWritten;

        bool TryGetInstanceBag(int instanceIndex, out InstanceDataBag bag);
        void WriteInstanceData(int instanceIndex, string packerId, byte[] data);
        bool TryDeleteInstance(int instanceIndex);
        InstanceDataBag CreateInstanceDataBag(int instanceIndex, string presetUid);
        bool HasInstance(int instanceIndex);
    }
}