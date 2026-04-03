// Copyright 2026 Spellbound Studio Inc.

namespace Spellbound.Core {
    public interface IEventSurfaceStore {
        bool HasIndex(int instancesIndex);

        void Register(int instanceIndex, EventSurface surface = null);
        
        bool Unregister(int instanceIndex);
        
        bool TryGetEventSurface(int instanceIndex, out EventSurface surface);
    }
}