using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Spellbound.Core {
    [Serializable]
    public abstract class MouseoverModule {
        public abstract Task OnMouseover(ObjectPreset contextObject = null, Dictionary<string, byte[]> data = null);
    }
}