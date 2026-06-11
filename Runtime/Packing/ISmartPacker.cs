// Copyright 2026 Spellbound Studio Inc.

using System;

namespace Spellbound.Core.Packing {
    /// <summary>
    /// A packer whose payload can be reconstructed without knowing the type first: SmartPack prefixes the
    /// payload with the type's stable hash, declared via <see cref="PackerIdAttribute"/>. Id and hash resolve
    /// through <see cref="PackerRegistry"/> — implementors declare nothing beyond the attribute.
    /// </summary>
    public interface ISmartPacker : IPacker {
        string PackerId => PackerRegistry.GetId(GetType());

        uint PackerHash => PackerRegistry.GetHash(GetType());

        void SmartPack(ref Span<byte> buffer) {
            Packer.WriteUInt(ref buffer, PackerHash);
            Pack(ref buffer);
        }

        static ISmartPacker SmartUnpack(byte[] data) {
            ReadOnlySpan<byte> span = data;
            return ISmartPacker.SmartUnpack(ref span);
        }

        static ISmartPacker SmartUnpack(ref ReadOnlySpan<byte> buffer) {
            var hash = Packer.ReadUInt(ref buffer);

            if (!PackerRegistry.TryCreateInstance(hash, out var instance))
                throw new Exception($"No ISmartPacker registered for hash: {hash}");

            var remaining = buffer;
            instance.Unpack(ref remaining);
            buffer = remaining;

            return instance;
        }
    }
}
