// Copyright 2026 Spellbound Studio Inc.

using System;
using Spellbound.Core.ObjectHandling;
using Spellbound.Core.Objects;
using Spellbound.Core.Packing;

namespace Spellbound.Core.ObjectData {
    /// <summary>
    /// Represents something that can not only be packed and unpacked, but can be unpacked without knowing what it is first.
    /// Methods with Invoke in the name are boilerplate required to be able to call generics with the concrete class.
    /// </summary>
    public interface IPackerObjectData : ISmartPacker {
        
        static IPackerObjectData SmartUnpackObjectData(ref ReadOnlySpan<byte> buffer) {
            var result = ISmartPacker.SmartUnpack(ref buffer);
    
            if (result is not IPackerObjectData objectData)
                throw new Exception($"PackerRegistry: '{result.PackerId}' is not an IPackerObjectData.");
    
            return objectData;
        }
        
        static IPackerObjectData SmartUnpackObjectData(byte[] data) {
            ReadOnlySpan<byte> span = data;
            return IPackerObjectData.SmartUnpackObjectData(ref span);
        }
        IPackerObjectData GetEmptyData();
        IPackerObjectData InvokeApplyDelta(IPackerObjectData delta, ObjectPreset preset, int surfaceIndex, out byte context);
        IPackerObjectData InvokeGetDefaultData(ObjectPreset preset, int surfaceIndex, byte level = 1);

        void InvokeChangeCallback(
            byte context, ObjectParent parent, int instanceIndex,
            ObjectPreset preset, int surfaceIndex, TransformData transformData);

        void InvokeResolveCallback(
            byte context, ObjectParent parent, int instanceIndex,
            ObjectPreset preset, int surfaceIndex, TransformData transformData);
        
    }
}