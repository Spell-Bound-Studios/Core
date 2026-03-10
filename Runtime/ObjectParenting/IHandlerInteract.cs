using UnityEngine;

namespace Spellbound.Core {
    public interface IHandlerInteract {
        void OnInteract(InteractContext ctx);
    }

    public readonly struct InteractContext {
        public readonly ObjectPreset Tool;
        public readonly GameObject Requestor;
        public readonly IObjectParentChunk Chunk;
        public readonly int EntityIndex;

        public InteractContext(ObjectPreset tool, GameObject requestor, IObjectParentChunk chunk, int entityIndex) {
            Tool = tool;
            Requestor = requestor;
            Chunk = chunk;
            EntityIndex = entityIndex;
        }
    }

}
