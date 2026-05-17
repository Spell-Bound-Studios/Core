// Copyright 2026 Spellbound Studio Inc.

using Spellbound.Core.ObjectData;
using Spellbound.Core.ObjectHandling;

namespace Spellbound.Core.PresetContracts {
    /// <summary>
    /// For ObjectPreset PresetModules
    /// Interface Contract for a Module to respond to changes with major consequences
    /// Typically context will play a large role in the response, via switch statement or ifs/elses.
    /// </summary>
    public interface IChangeResolver<T>  where T : IDecodableData{
        void ResolveChange(T data, byte context, ObjectParent parent, int instanceIndex, TransformData transformData);
    }
}