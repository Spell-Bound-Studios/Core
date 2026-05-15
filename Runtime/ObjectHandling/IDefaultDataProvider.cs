// Copyright 2026 Spellbound Studio Inc.

namespace Spellbound.Core {
    /// <summary>
    /// For ObjectPreset PresetModules
    /// Interface Contract for a Module to provide the default data of the ObjectPreset it is on.
    /// Level is scaffolding for future "1star", "2star", etc variants of an Enemy
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IDefaultDataProvider<T> where T : IDecodableData {
        T GetDefaultData(ObjectPreset preset, byte level = 1);
    }
}