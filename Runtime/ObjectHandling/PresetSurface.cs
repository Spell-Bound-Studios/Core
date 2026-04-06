// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spellbound.Core {
    [Serializable]
    public class PresetSurface {
        public string surfaceName;
        
        [SerializeReference] public List<PresetModule> presetModules;
    }
}