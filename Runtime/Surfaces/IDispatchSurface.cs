// Copyright 2026 Spellbound Studio Inc.

using System.Threading.Tasks;
using Spellbound.Core.Packing;

namespace Spellbound.Core.Surfaces {
    public interface IDispatchSurface {
        Task<int> Dispatch<TDispatch>(TDispatch dispatch)
                where TDispatch : struct, IPacker;

    }
}