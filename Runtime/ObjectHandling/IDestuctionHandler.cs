// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using Spellbound.Core.Packing;

namespace Spellbound.Core {
    public interface IDestuctionHandler {
        void OnDestruction(
            int instanceIndex, ObjectParent parent, out List<Action<int, TransformData>> actions);
    }
}