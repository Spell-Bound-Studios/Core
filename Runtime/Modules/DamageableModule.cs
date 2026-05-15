// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Spellbound.Core.Objects;
using UnityEngine;

namespace Spellbound.Core.Modules {
    [Serializable]
    public abstract class DamageableModule : PresetModule {
        // the ctx must include the damage that's being dealt
        // no need for authoritative control of the data; we can tolerate some "overkill"
        public abstract Task OnDamage(ObjectPreset ctx, GameObject attacker, Dictionary<string, byte[]> data);
    }
}