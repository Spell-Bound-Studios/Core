// Copyright 2026 Spellbound Studio Inc.

namespace Spellbound.Core {
    public interface IResolveHandler<T> where T : IDecodableData {
        void OnResolve(T data, byte context, IObjectDataAccess dataAccess, int instanceIndex, ObjectPreset preset, int surfaceIndex, TransformData transformData);
    }
}