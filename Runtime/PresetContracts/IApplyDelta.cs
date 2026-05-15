// Copyright 2026 Spellbound Studio Inc.

using Spellbound.Core.ObjectData;

namespace Spellbound.Core.PresetContracts {
    public interface IApplyDelta<TData, TDelta> where TData : IDecodableData {
        TData ApplyDelta(TData current, TDelta delta, out byte context);
    }
}