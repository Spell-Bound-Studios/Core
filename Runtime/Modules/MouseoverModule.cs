// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Spellbound.Core.Objects;

namespace Spellbound.Core.Modules {
    [Serializable]
    public abstract class MouseoverModule : PresetModule {
        public abstract Task OnMouseover(ObjectPreset contextObject = null, Dictionary<string, byte[]> data = null);
    }
}