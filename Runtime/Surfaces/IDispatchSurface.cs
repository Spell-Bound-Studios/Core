// Copyright 2026 Spellbound Studio Inc.

using Spellbound.Core.Packing;

namespace Spellbound.Core.Surfaces {
    public interface IDispatchSurface {
        void Dispatch<TDispatch>(TDispatch dispatch)
                where TDispatch : struct, IPacker;

    }
}