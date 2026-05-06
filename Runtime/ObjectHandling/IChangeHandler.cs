// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using Spellbound.Core.Packing;
using UnityEngine;

namespace Spellbound.Core {
    public interface IChangeHandler {
        void OnChange(IPacker data, int instanceIndex, ObjectParent parent, out List<Action<int, Transform>> structuralActions, out List<Action<int, Transform>> cosmeticActions);
    }
}