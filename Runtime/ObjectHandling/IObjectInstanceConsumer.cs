// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using Spellbound.Core.Packing;

namespace Spellbound.Core {
    public interface IObjectInstanceConsumer {
        int GetNextInstanceIndex();
        void OnRuntimeInstancesLoaded(IReadOnlyList<(int, NonProceduralStaticInstanceEntry)> creations);
        void OnRuntimeInstancesCreated(IReadOnlyList<(int, NonProceduralStaticInstanceEntry)> creations);
        void OnDynamicObjectsLoaded(IReadOnlyList<(int, DynamicInstanceEntry)> loaded);
        void OnDynamicObjectsCreated(IReadOnlyList<(int, DynamicInstanceEntry)> creations);
        void OnInstancesDeleted(IReadOnlyList<int> instanceIndices);
        void OnInstanceDataStructuralChanged(int instanceIndex, InstanceDataKey key, Func<IPacker> dataFunc, Type handler);
        
        void OnInstanceDataCosmeticChanged(int instanceIndex, InstanceDataKey key, Func<IPacker> dataFunc, Type handler);
        
        void OnInstanceDataInitialized(int instanceIndex, InstanceDataKey key, Func<IPacker> dataFunc);
    }
}