// Copyright 2025 Spellbound Studio Inc.

using System;

namespace Spellbound.Core {
    /// <summary>
    /// Purposely empty so that inheritors can describe their own uniqueness.
    /// </summary>
    [Serializable]
    public abstract class PresetModule {
        public abstract SbbData? GetData(ObjectPreset preset);
    }
}