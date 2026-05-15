// Copyright 2026 Spellbound Studio Inc.

using Spellbound.Core.ObjectData;

namespace Spellbound.Core.ObjectHandling {
    /// <summary>
    /// TODO:
    /// </summary>
    public interface IQuantitativeData : IDecodableData {
        IQuantitativeData ApplyDelta(IQuantitativeData delta, out byte context, byte? contextIn);
    }
}