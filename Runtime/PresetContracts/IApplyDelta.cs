// Copyright 2026 Spellbound Studio Inc.

using Spellbound.Core.ObjectData;
using Spellbound.Core.Objects;

namespace Spellbound.Core.PresetContracts {
    /// <summary>
    /// For ObjectPreset PresetModules. Contract for taking the current data of an instance and applying an
    /// incoming delta of the same shape, returning the new data plus a context byte for downstream change /
    /// resolve handlers.
    /// </summary>
    public interface IApplyDelta<T> where T : IDecodableData {
        T ApplyDelta(T data, T delta, ObjectPreset preset, int surfaceIndex, out byte context);
    }
}
