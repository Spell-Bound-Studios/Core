// Copyright 2026 Spellbound Studio Inc.

using System;
using Spellbound.Core.Packing;

namespace Spellbound.Core {
    public interface IHandlerThreshold {
        bool ThresholdCheck(IQuantitativeData data, out Action<IObjectDataAccess, int> thresholdAction);
        IPacker GetDefaultData();
    }
}