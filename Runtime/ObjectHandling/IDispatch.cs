// Copyright 2026 Spellbound Studio Inc.

namespace Spellbound.Core {
    /// <summary>
    /// For ObjectPreset PresetModules
    /// Interface Contract for a Module to recieve a dispatch event from an event surface.
    /// Dispatch is typically the call site for something that ultimately changes some data.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IDispatch<T> where T : struct {
        bool OnDispatch(
            T dispatchContext, IObjectParent parent, int instanceIndex, ObjectPreset preset, int eventSurfaceIndex);
    }
}