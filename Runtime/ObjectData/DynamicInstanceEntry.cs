// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using Spellbound.Core.Packing;

namespace Spellbound.Core.ObjectData {
    public class DynamicInstanceEntry : IPacker {
        public string PresetUid;
        public Dictionary<InstanceDataKey, byte[]> DataSlots = new();
        public TransformData Transform;
        public bool WasMovingAtSave;

        public DynamicInstanceEntry() { }

        /// <summary>
        /// Ctor for DynamicInstanceEntry.
        /// </summary>
        /// <param name="presetUid"></param>
        /// <param name="transform"></param>
        /// <param name="wasMovingAtSave"></param>
        public DynamicInstanceEntry(string presetUid, TransformData transform, bool wasMovingAtSave) {
            PresetUid = presetUid;
            Transform = transform;
            WasMovingAtSave = wasMovingAtSave;
        }

        public void Pack(ref Span<byte> buffer) {
            Packer.WriteString(ref buffer, PresetUid);
            Transform.Pack(ref buffer);
            Packer.WriteBool(ref buffer, WasMovingAtSave);

            Packer.WriteInt(ref buffer, DataSlots.Count);

            foreach (var (key, bytes) in DataSlots) {
                Packer.WriteString(ref buffer, key.PackerId);
                Packer.WriteInt(ref buffer, key.SurfaceIndex);
                Packer.WriteBytes(ref buffer, bytes);
            }
        }

        public void Unpack(ref ReadOnlySpan<byte> buffer) {
            PresetUid = Packer.ReadString(ref buffer);
            Transform = new TransformData();
            Transform.Unpack(ref buffer);
            WasMovingAtSave = Packer.ReadBool(ref buffer);

            var count = Packer.ReadInt(ref buffer);
            DataSlots = new Dictionary<InstanceDataKey, byte[]>(count);

            for (var i = 0; i < count; i++) {
                var packerId = Packer.ReadString(ref buffer);
                var surfaceIndex = Packer.ReadInt(ref buffer);
                var bytes = Packer.ReadBytes(ref buffer);
                DataSlots[new InstanceDataKey(packerId, surfaceIndex)] = bytes;
            }
        }
    }
}