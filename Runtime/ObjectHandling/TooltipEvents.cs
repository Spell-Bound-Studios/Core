// Copyright 2026 Spellbound Studio Inc.

using System;
using UnityEngine;

namespace Spellbound.Core {
    public class TooltipEvents {
        public static event Action<string> OnTooltipChanged;
        public static void Invoke(string tooltip) => OnTooltipChanged?.Invoke(tooltip);
    }
}