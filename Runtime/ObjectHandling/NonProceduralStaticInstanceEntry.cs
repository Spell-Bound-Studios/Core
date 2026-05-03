// Copyright 2026 Spellbound Studio Inc.

using System.Collections.Generic;

namespace Spellbound.Core {
    public class NonProceduralStaticInstanceEntry {
        public readonly string PresetUid;
        public readonly Dictionary<InstanceDataKey, byte[]> DataSlots = new();
        public TransformData Transform;

        public NonProceduralStaticInstanceEntry(string presetUid, TransformData transform) {
            PresetUid = presetUid;
            Transform = transform;
        }
    }
}