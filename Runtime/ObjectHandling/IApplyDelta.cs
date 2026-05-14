// Copyright 2026 Spellbound Studio Inc.

namespace Spellbound.Core {
    public interface IApplyDelta<TData, TDelta> where TData : IDecodableData {
        TData ApplyDelta(TData current, TDelta delta, out byte context);
    }
}