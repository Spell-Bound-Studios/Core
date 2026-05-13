// Copyright 2026 Spellbound Studio Inc.

namespace Spellbound.Core {
    public interface IDispatch<T> where T : struct {
        bool OnDispatch(T dispatchContext, IObjectParent parent, int instanceIndex, ObjectPreset preset, int eventSurfaceIndex);
    }
}