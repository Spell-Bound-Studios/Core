// Copyright 2026 Spellbound Studio Inc.

using System;
using Spellbound.Core.Packing;

namespace Spellbound.Core {
    public interface IDecodableData : IPacker {
        string PackerId { get; }

        IDecodableData GetDefaultData();
        
        static IDecodableData Decode(string packerId, byte[] data) {
            if (!PackerRegistry.TryCreateInstance(packerId, out var instance))
                throw new Exception($"No blank registered for packerId: {packerId}");
            
            ReadOnlySpan<byte> span = data;
            instance.Unpack(ref span);
            return instance;
        }
        
        void Callback(byte context, ObjectPreset preset, int surfaceIndex);
    }
}