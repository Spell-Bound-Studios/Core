// Copyright 2026 Spellbound Studio Inc.

using System;
using Spellbound.Core.Packing;

namespace Spellbound.Core {
    /// <summary>
    /// Represents something that can not only be packed and unpacked, but can be unpacked without knowing what it is first.
    /// Methods with Invoke in the name are boilerplate required to be able to call generics with the concrete class.
    /// </summary>
    public interface IDecodableData : IPacker {
        string PackerId { get; }

        IDecodableData GetEmptyData();

        // TODO, delete it to be handled by IDefaultDataProvider<T>
        IDecodableData InvokeGetDefaultData(ObjectPreset preset, int surfaceIndex, byte level = 1);

        void InvokeChangeCallback(
            byte context, ObjectParent parent, int instanceIndex,
            ObjectPreset preset, int surfaceIndex, TransformData transformData);

        void InvokeResolveCallback(
            byte context, ObjectParent parent, int instanceIndex,
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