// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using Spellbound.Core.Packing;

namespace Spellbound.Core {
    public interface IObjectInstanceConsumer {
        void OnRuntimeInstancesCreated(IReadOnlyList<(int, NonProceduralStaticInstanceEntry)> creations);

        void OnRuntimeInstancesCreatedNew(IReadOnlyList<(int, NonProceduralStaticInstanceEntry)> creations);
        void OnInstancesDeleted(IReadOnlyList<int> instanceIndices);
        void OnInstanceDataStructuralChanged(int instanceIndex, InstanceDataKey key, Func<IPacker> dataFunc, Type handler);
        
        void OnInstanceDataCosmeticChanged(int instanceIndex, InstanceDataKey key, Func<IPacker> dataFunc, Type handler);
        
        void OnInstanceDataInitialized(int instanceIndex, InstanceDataKey key, Func<IPacker> dataFunc);
    }
}