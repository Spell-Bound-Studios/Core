// Copyright 2026 Spellbound Studio Inc.

using System.Collections.Generic;

namespace Spellbound.Core {
    public class InstanceEntry {
        public readonly string PresetUid;
        public readonly Dictionary<InstanceDataKey, byte[]> DataSlots;
        public TransformData? Transform;

        public InstanceEntry(string presetUid) {
            PresetUid = presetUid;
            DataSlots = new Dictionary<InstanceDataKey, byte[]>();
            Transform = null;
        }
    }
}