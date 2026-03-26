// Copyright 2026 Spellbound Studio Inc.

using System;
using Spellbound.Core.Packing;

namespace Spellbound.Core {
    public interface IQuantitativeData : IPacker {
        IQuantitativeData ApplyDelta(IQuantitativeData delta);
    }
}