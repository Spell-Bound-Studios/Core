using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Spellbound.Core {
    public class Ballsack {
        // ISbDatas (type TBD because Dictionary<Dictionary<string, byte[]>> is sus)
        // ISbDeletions (Hashset<int>)
        // Dictionary<int, EventSurface> ProxyDict
        public event Action<Bounds> ReParentingCheck;
        
        // Lots of methods subscribed to OnChanged events
    }
}