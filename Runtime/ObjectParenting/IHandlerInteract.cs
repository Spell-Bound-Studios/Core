using UnityEngine;

namespace Spellbound.Core {
    public interface IHandlerInteract {
        void OnInteract(InteractContext ctx);
    }

    public readonly struct InteractContext {
        public readonly ObjectPreset Tool;
        public readonly GameObject Requestor;
        public readonly IObjectParent Chunk;
        public readonly int EntityIndex;

        public InteractContext(ObjectPreset tool, GameObject requestor, IObjectParent chunk, int entityIndex) {
            Tool = tool;
            Requestor = requestor;
            Chunk = chunk;
            EntityIndex = entityIndex;
        }
    }

}
