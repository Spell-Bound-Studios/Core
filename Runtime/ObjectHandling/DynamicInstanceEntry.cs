// Copyright 2026 Spellbound Studio Inc.

using System.Collections.Generic;

namespace Spellbound.Core {
    public class DynamicInstanceEntry {
        public readonly string PresetUid;
        public readonly Dictionary<InstanceDataKey, byte[]> DataSlots = new();
        public TransformData Transform;
        public bool WasMovingAtSave;

        public DynamicInstanceEntry(string presetUid, TransformData transform, bool wasMovingAtSave) {
            PresetUid = presetUid;
            Transform = transform;
            WasMovingAtSave = wasMovingAtSave;
        }
    }
}