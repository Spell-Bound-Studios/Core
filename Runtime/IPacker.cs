using System;

namespace SpellBound.Core {
    public interface IPacker {
        public void Pack(ref Span<byte> buffer);
        public void Unpack(ref ReadOnlySpan<byte> buffer);
    }
}