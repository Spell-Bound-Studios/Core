// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;

namespace Spellbound.Core {
    public interface ICreationHandler {
        void OnCreate(int instanceIndex, ObjectParent parent, out List<Action<int, TransformData>> actions);
    }
}