// Copyright 2026 Spellbound Studio Inc.

using System;

namespace Spellbound.Core {
    public interface IHandlerThreshold {
        bool ThresholdCheck(IQuantitativeData data, out Action<IObjectDataStore, int> thresholdAction);
        IQuantitativeData GetDefaultData();
    }
}