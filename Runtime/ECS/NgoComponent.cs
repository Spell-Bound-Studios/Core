using Unity.Collections;
using Unity.Entities;

namespace SpellBound.Core {
    public struct NgoComponent : IComponentData {
        public FixedString64Bytes Id;
    }
}