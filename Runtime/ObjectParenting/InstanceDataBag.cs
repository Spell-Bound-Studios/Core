// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using Spellbound.Core.Packing;

namespace Spellbound.Core {
    public class InstanceDataBag : IPacker {
        public InstanceDataBag(string presetUid) {
            _presetUid = presetUid;
        }
        
        #region Storage
        
        private readonly string _presetUid;
        private readonly Dictionary<string, byte[]> _data = new();

        public bool IsDirty { get; private set; }

        #endregion

        #region Read / Write

        public void Write(string packerId, byte[] bytes) {
            _data[packerId] = bytes;
            IsDirty = true;
        }

        public bool TryRead(string packerId, out byte[] bytes) {
            return _data.TryGetValue(packerId, out bytes);
        }

        public void ClearDirty() => IsDirty = false;

        #endregion

        #region IPacker

        public void Pack(ref Span<byte> buffer) {
            Packer.WriteInt(ref buffer, _data.Count);

            foreach (var kvp in _data) {
                Packer.WriteString(ref buffer, kvp.Key);
                Packer.WriteBytes(ref buffer, kvp.Value);
            }
        }

        public void Unpack(ref ReadOnlySpan<byte> buffer) {
            _data.Clear();
            var count = Packer.ReadInt(ref buffer);

            for (var i = 0; i < count; i++) {
                var key = Packer.ReadString(ref buffer);
                var data = Packer.ReadBytes(ref buffer);
                _data[key] = data;
            }

            IsDirty = false;
        }

        #endregion

        #region Convenience

        /// <summary>Convenience wrapper over Packer.ToBytes.</summary>
        public byte[] Serialize() => Packer.ToBytes(this);

        /// <summary>Convenience wrapper over Unpack.</summary>
        public void Deserialize(byte[] raw) {
            ReadOnlySpan<byte> span = raw;
            Unpack(ref span);
        }

        #endregion
    }
}