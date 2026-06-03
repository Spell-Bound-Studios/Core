// Copyright 2026 Spellbound Studio Inc.

using System;

namespace Spellbound.Core.Packing {
    public interface ISmartPacker : IPacker {
        string PackerId { get; }

        void SmartPack(ref Span<byte> buffer) {
            Packer.WriteUShort(ref buffer, PackerRegistry.GetHash(PackerId));
            Pack(ref buffer);
        }
        
        static ISmartPacker SmartUnpack(byte[] data) {
            ReadOnlySpan<byte> span = data;
            return ISmartPacker.SmartUnpack(ref span);
        }

        static ISmartPacker SmartUnpack(ref ReadOnlySpan<byte> buffer) {
            var hash = Packer.ReadUShort(ref buffer);

            if (!PackerRegistry.TryCreateInstance(hash, out var instance))
                throw new Exception($"No ISmartPacker registered for hash: {hash}");

            var remaining = buffer;
            instance.Unpack(ref remaining);
            buffer = remaining;

            return instance;
        }
    }
}