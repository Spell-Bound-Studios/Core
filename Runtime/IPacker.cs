// Copyright 2025 Spellbound Studio Inc.

using System;

namespace Spellbound.Core {
    public interface IPacker {
        public void Pack(ref Span<byte> buffer);
        public void Unpack(ref ReadOnlySpan<byte> buffer);
    }
}