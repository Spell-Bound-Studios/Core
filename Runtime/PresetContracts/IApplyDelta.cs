// Copyright 2026 Spellbound Studio Inc.

using Spellbound.Core.ObjectData;
using Spellbound.Core.Objects;
using Spellbound.Core.Packing;

namespace Spellbound.Core.PresetContracts {
    public interface IApplyDelta<T, TDispatch> 
            where T : IPackerObjectData
            where TDispatch : IPackerDispatch {
        T ApplyDelta(T data, TDispatch delta, ObjectPreset preset, int surfaceIndex, out ISmartPacker consequence);
    }
}