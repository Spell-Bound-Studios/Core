using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Spellbound.Core {
    [Serializable]
    public abstract class InteractableModule : PresetModule {
        // ctxObject is the Held Item
        public abstract Task OnInteract(ObjectPreset ctxObject, GameObject requestor, Dictionary<string, byte[]> data);
        
    }
}
