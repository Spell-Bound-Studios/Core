// Copyright 2026 Spellbound Studio Inc.

namespace Spellbound.Core {
    public interface ITimerModule {
        void OnTimerUpdate(
            uint time,
            IObjectDataAccess dataAccess, int instanceIndex, ObjectPreset preset);
    }

    public interface ITimerModule<T> : ITimerModule where T : IDecodableData {
        void OnTimerUpdate(
            T data, uint time, IObjectDataAccess dataAccess,
            int instanceIndex, ObjectPreset preset, int eventSurfaceIndex = 0);
    }
}