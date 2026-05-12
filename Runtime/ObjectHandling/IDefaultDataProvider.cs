// Copyright 2026 Spellbound Studio Inc.

using Spellbound.Core.Packing;

namespace Spellbound.Core {
    public interface IDefaultDataProvider<T> where T : IDecodableData {
        T GetDefaultData(ObjectPreset preset, byte level = 1);
    }
}