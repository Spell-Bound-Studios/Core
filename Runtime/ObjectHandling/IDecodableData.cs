// Copyright 2026 Spellbound Studio Inc.

using System;
using Spellbound.Core.Logging;
using Spellbound.Core.Packing;

namespace Spellbound.Core {
    public interface IDecodableData : IPacker {
        string PackerId { get; }

        IDecodableData GetEmptyData();
        
        IDecodableData InvokeGetDefaultData(ObjectPreset preset, int surfaceIndex, byte level = 1);
        
        void InvokeChangeCallback(byte context, IObjectDataAccess dataAccess, int instanceIndex, 
            ObjectPreset preset, int surfaceIndex, TransformData transformData);
        void InvokeResolveCallback(byte context, IObjectDataAccess dataAccess, int instanceIndex,
            ObjectPreset preset, int surfaceIndex, TransformData transformData);
        
        static IDecodableData Decode(string packerId, byte[] data) {
            if (!PackerRegistry.TryCreateInstance(packerId, out var instance))
                throw new Exception($"No blank registered for packerId: {packerId}");
            
            ReadOnlySpan<byte> span = data;
            instance.Unpack(ref span);
            return instance;
        }
    }
}