// Copyright 2025 Spellbound Studio Inc.

using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Spellbound.Core {
    public interface IRangeCheck {
        Task RangeCheckLoop(Transform requester, CancellationToken token);
    }
}