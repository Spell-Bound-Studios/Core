// Copyright 2025 Spellbound Studio Inc.

using System;

namespace Spellbound.Core {
    /// <summary>
    /// This is the base class of all PresetModule types that users can implement and create. All Spellbound packages
    /// make use of this class.
    /// Purposely empty so that inheritors can describe their own uniqueness.
    /// </summary>
    [Serializable]
    public abstract class PresetModule {
        public abstract SbbData? GetData(ObjectPreset preset);
    }
}