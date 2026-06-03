// Copyright 2026 Spellbound Studio Inc.

using Spellbound.Core.ObjectData;
using Spellbound.Core.Objects;

namespace Spellbound.Core.PresetContracts {
    public interface IApplyDelta<T> where T : IPackerObjectData{
        T ApplyDelta(T data, T delta, ObjectPreset preset, int surfaceIndex, out byte context);
    }
}