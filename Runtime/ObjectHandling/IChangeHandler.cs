// Copyright 2026 Spellbound Studio Inc.

namespace Spellbound.Core {
    public interface IChangeHandler<T> where T : IDecodableData {
        void OnChange(T data, byte context, IObjectDataAccess dataAccess, int instanceIndex, ObjectPreset preset, int surfaceIndex, TransformData transformData);
    }
}