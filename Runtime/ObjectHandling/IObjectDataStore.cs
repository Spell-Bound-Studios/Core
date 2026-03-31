// Copyright 2026 Spellbound Studio Inc.

using System;

namespace Spellbound.Core {
    public interface IObjectDataStore {
        event Action<int> OnInstanceCreated;
        event Action<int> OnInstanceRemoved;
        event Action<int, (string, int)> OnInstanceDataChanged; // instanceIdx, packerId

        void CreateInstance(int instanceIndex, string presetUid);
        bool HasInstance(int instanceIndex);
        string GetPresetUid(int instanceIndex);
    
        bool TryRead(int instanceIndex, string packerId, int eventSurfaceIndex, out byte[] data);
        void Write(int instanceIndex, string packerId, int eventSurfaceIndex, byte[] data);

        void Delta<T>(int instanceIndex, string presetUid, string packerId, int eventSurfaceIndex, T delta)
                where T : IQuantitativeData, new();
        bool TryDeleteInstance(int instanceIndex);
    }
}