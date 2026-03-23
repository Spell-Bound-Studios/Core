// Copyright 2026 Spellbound Studio Inc.

using System;

namespace Spellbound.Core {
    public interface IObjectDataStore {
        event Action<int> OnInstanceCreated;
        event Action<int> OnInstanceRemoved;
        event Action<int, string> OnInstanceDataChanged; // instanceIdx, packerId

        void CreateInstance(int instanceIndex, string presetUid);
        bool HasInstance(int instanceIndex);
        string GetPresetUid(int instanceIndex);
    
        bool TryRead(int instanceIndex, string packerId, out byte[] data);
        void Write(int instanceIndex, string packerId, byte[] data);
        bool TryDeleteInstance(int instanceIndex);
    }
}