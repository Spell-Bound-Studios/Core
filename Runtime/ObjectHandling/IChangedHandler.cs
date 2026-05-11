// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using Spellbound.Core.Packing;
using UnityEngine;

namespace Spellbound.Core {
    public interface IChangedHandler<T> where T : IDecodableData {
        void OnSet(T data);
        void OnGain(T data);
        void OnLoss(T data);
    }
}