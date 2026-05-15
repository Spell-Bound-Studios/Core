// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using Spellbound.Core.Packing;

namespace Spellbound.Core {
    public interface IThresholdHandler {
        bool ThresholdCheck(IQuantitativeData data, ObjectParent parent, out List<Action<int, TransformData>> actions);
    }
}