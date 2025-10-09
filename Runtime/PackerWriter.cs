using System;
using UnityEngine;

namespace SpellBound.Core {
    /// <summary>
    /// High-level, chainable wrapper for writing binary data into a Span.
    /// Uses the static Packer methods internally.
    /// </summary>
    public ref struct PackerWriter {
        private Span<byte> _buffer;

        public PackerWriter(Span<byte> buffer) {
            _buffer = buffer;
        }

        public readonly int BytesWritten => initialLength - _buffer.Length;
        private static int initialLength => 4096;

        // Expose the remaining buffer for advanced users
        public readonly Span<byte> Remaining => _buffer;

        public PackerWriter Write(bool value) {
            Packer.WriteBool(ref _buffer, value);

            return this;
        }

        public PackerWriter Write(int value) {
            Packer.WriteInt(ref _buffer, value);

            return this;
        }

        public PackerWriter Write(float value) {
            Packer.WriteFloat(ref _buffer, value);

            return this;
        }

        public PackerWriter Write(string value) {
            Packer.WriteString(ref _buffer, value);

            return this;
        }

        public PackerWriter Write(Vector2 value) {
            Packer.WriteVector2(ref _buffer, value);

            return this;
        }

        public PackerWriter Write(Vector3 value) {
            Packer.WriteVector3(ref _buffer, value);

            return this;
        }

        public PackerWriter Write(Quaternion value) {
            Packer.WriteQuaternion(ref _buffer, value);

            return this;
        }

        public PackerWriter WriteBytes(ReadOnlySpan<byte> data) {
            Packer.WriteInt(ref _buffer, data.Length);
            data.CopyTo(_buffer);
            _buffer = _buffer[data.Length..];

            return this;
        }

        public byte[] ToArray() {
            var written = 4096 - _buffer.Length;

            return _buffer[..written].ToArray();
        }
    }
}