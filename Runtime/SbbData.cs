// Copyright 2025 Spellbound Studio Inc.

using System;

namespace Spellbound.Core {
    /// <summary>
    /// 
    /// </summary>
    public struct SbbData : IPacker {
        public string Id;
        public byte[] PackedData;

        public override string ToString() => $"SbbData ▶ Id={Id}, Bytes={PackedData?.Length ?? 0}";

        public void Pack(ref Span<byte> buffer) {
            Packer.WriteString(ref buffer, Id);
            Packer.WriteBytes(ref buffer, PackedData);
        }

        public void Unpack(ref ReadOnlySpan<byte> buffer) {
            Id = Packer.ReadString(ref buffer);
            PackedData = Packer.ReadBytes(ref buffer);
        }
    }
}