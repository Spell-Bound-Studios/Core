// Copyright 2026 Spellbound Studio Inc.

using Spellbound.Core.Packing;

namespace Spellbound.Core {
    public interface IQuantitativeData : IDecodableData {
        IQuantitativeData ApplyDelta(IQuantitativeData delta);
    }
}