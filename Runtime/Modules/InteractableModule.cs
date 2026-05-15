// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Spellbound.Core.Objects;
using UnityEngine;

namespace Spellbound.Core.Modules {
    [Serializable]
    public abstract class InteractableModule : PresetModule {
        // ctxObject is the Held Item
        public abstract Task OnInteract(ObjectPreset ctxObject, GameObject requestor, Dictionary<string, byte[]> data);
    }
}