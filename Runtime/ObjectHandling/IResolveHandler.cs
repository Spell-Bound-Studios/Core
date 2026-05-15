// Copyright 2026 Spellbound Studio Inc.

namespace Spellbound.Core {
    /// <summary>
    /// For ObjectPreset PresetModules
    /// Interface Contract for a Module to respond to changes with major consequences
    /// Typically context will play a large role in the response, via switch statement or ifs/elses.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IResolveHandler<T> where T : IDecodableData {
        void OnResolve(
            T data, byte context, ObjectParent parent, int instanceIndex, ObjectPreset preset,
            int surfaceIndex, TransformData transformData);
    }
}