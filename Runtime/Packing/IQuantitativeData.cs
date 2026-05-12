// Copyright 2026 Spellbound Studio Inc.


namespace Spellbound.Core {
    public interface IQuantitativeData : IDecodableData {
        IQuantitativeData ApplyDelta(IQuantitativeData delta, out byte context, byte? contextIn);
    }
}