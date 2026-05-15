// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using Spellbound.Core.ObjectHandling;

namespace Spellbound.Core.PresetContracts {
    public interface ICreationHandler {
        void OnCreation(int instanceIndex, ObjectParent parent, out List<Action<int, TransformData>> actions);
    }
}