// Copyright 2026 Spellbound Studio Inc.

using Spellbound.Core.ObjectData;
using Spellbound.Core.ObjectHandling;
using Spellbound.Core.Objects;

namespace Spellbound.Core.PresetContracts {
    /// <summary>
    /// For ObjectPreset PresetModules
    /// Interface Contract for a Module to respond to changes with cosmetic/aesthetic/audio
    /// Typically context will play a large role in the response, via switch statement or ifs/elses.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IChangeHandler<T> where T : IDecodableData {
        void OnChange(
            T data, byte context, ObjectParent parent, int instanceIndex, ObjectPreset preset,
            int surfaceIndex, TransformData transformData);
    }
}