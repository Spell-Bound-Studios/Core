// Copyright 2026 Spellbound Studio Inc.

namespace Spellbound.Core {
    public readonly struct RuntimeInstanceCreation {
        public readonly int InstanceIndex;
        public readonly string PresetUid;
        public readonly TransformData Transform;

        public RuntimeInstanceCreation(int instanceIndex, string presetUid, TransformData transform) {
            InstanceIndex = instanceIndex;
            PresetUid = presetUid;
            Transform = transform;
        }
    }
}