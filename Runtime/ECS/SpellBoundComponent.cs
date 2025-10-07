using Unity.Collections;
using Unity.Entities;

namespace SpellBound.Core {
    public struct SpellBoundComponent : IComponentData {
        public FixedString64Bytes PresetUiD; //uid, name, and description should be a blobassetreference to save memory
        public int GenerationIndex;
    }
}
