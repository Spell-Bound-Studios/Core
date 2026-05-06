// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using Spellbound.Core.Packing;
using UnityEngine;

namespace Spellbound.Core {
    public interface IChangeHandler {
        void OnChangeStructural(IPacker data, int instanceIndex, ObjectParent parent, out List<Action<int, TransformData>> actions);
        
        void OnChangeCosmetic(IPacker data, int instanceIndex, ObjectParent parent, out List<Action<int, TransformData>> actions);
    }
}