// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using Spellbound.Core.Packing;

namespace Spellbound.Core {
    /// <summary>
    /// Snapshot of ONE INSTANCE's state at ONE point in time.
    /// What is the health of the tree RIGHT NOW. 
    /// </summary>
    public class InstanceDataBag : IPacker {

        #region Storage

        public string PresetUid { get; private set; }
        private readonly Dictionary<string, byte[]> _data = new();

        #endregion

        #region Constructors

        public InstanceDataBag() { }

        public InstanceDataBag(string presetUid) {
            PresetUid = presetUid;
        }

        #endregion

        #region Read / Write

        public void Write(string packerId, byte[] bytes) {
            _data[packerId] = bytes;
        }

        public bool TryRead(string packerId, out byte[] bytes) {
            return _data.TryGetValue(packerId, out bytes);
        }

        #endregion

        #region IPacker

        public void Pack(ref Span<byte> buffer) {
            Packer.WriteString(ref buffer, PresetUid ?? string.Empty);
            Packer.WriteInt(ref buffer, _data.Count);

            foreach (var kvp in _data) {
                Packer.WriteString(ref buffer, kvp.Key);
                Packer.WriteBytes(ref buffer, kvp.Value);
            }
        }

        public void Unpack(ref ReadOnlySpan<byte> buffer) {
            PresetUid = Packer.ReadString(ref buffer);
            _data.Clear();

            var count = Packer.ReadInt(ref buffer);

            for (var i = 0; i < count; i++) {
                var key = Packer.ReadString(ref buffer);
                var data = Packer.ReadBytes(ref buffer);
                _data[key] = data;
            }
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