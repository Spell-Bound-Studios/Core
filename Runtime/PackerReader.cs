using System;
using UnityEngine;

namespace SpellBound.Core {
    /// <summary>
    /// High-level, chainable wrapper for reading binary data from a ReadOnlySpan.
    /// Uses the static Packer methods internally.
    /// </summary>
    public ref struct PackerReader {
        private ReadOnlySpan<byte> _buffer;
        private readonly ReadOnlySpan<byte> _start;

        public PackerReader(ReadOnlySpan<byte> buffer) {
            _buffer = buffer;
            _start = buffer;
        }

        // How many bytes have been consumed so far
        public int Position => _start.Length - _buffer.Length;

        // Remaining readable bytes
        public ReadOnlySpan<byte> Remaining => _buffer;

        // ===== read helpers =====
        public bool ReadBool() {
            return Packer.ReadBool(ref _buffer);
        }

        public int ReadInt() {
            return Packer.ReadInt(ref _buffer);
        }

        public float ReadFloat() {
            return Packer.ReadFloat(ref _buffer);
        }

        public string ReadString() {
            return Packer.ReadString(ref _buffer);
        }

        public Vector2 ReadVector2() {
            return Packer.ReadVector2(ref _buffer);
        }

        public Vector3 ReadVector3() {
            return Packer.ReadVector3(ref _buffer);
        }

        public Quaternion ReadQuaternion() {
            return Packer.ReadQuaternion(ref _buffer);
        }

        // Returns a ReadOnlySpan<byte> slice of the next length-prefixed blob and advances the reader
        public ReadOnlySpan<byte> ReadBytes() {
            var len = Packer.ReadInt(ref _buffer);
            var segment = _buffer[..len];
            _buffer = _buffer[len..];

            return segment;
        }
    }
}