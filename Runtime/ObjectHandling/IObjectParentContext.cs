// Copyright 2026 Spellbound Studio Inc.

using System.Collections.Generic;

namespace Spellbound.Core {
    public interface IObjectParentContext {
        bool TryCreateInstance(int instanceIndex, string presetUid, TransformData transformData, out bool isDynamic);
        
        bool TryDeleteInstance(int instanceIndex);

       Dictionary<int, IEventSurface> DynamicEventSurfaceDict { get; }
        
        int ProceduralInstanceIndexCount { get; }

        bool IsCurrentlyDynamicOrProcedural(int instanceIndex) =>
                DynamicEventSurfaceDict.ContainsKey(instanceIndex) || instanceIndex < ProceduralInstanceIndexCount;

        void HandleFullState(Dictionary<int, InstanceEntry> instances, HashSet<int> deletions); 
    }
}