// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using Spellbound.Core.Packing;

namespace Spellbound.Core {
    public interface IObjectInstanceConsumer {
        int GetNextInstanceIndex();
        void OnRuntimeInstancesLoaded(IReadOnlyList<(int, NonProceduralStaticInstanceEntry)> creations);
        void OnRuntimeInstancesCreated(IReadOnlyList<(int, NonProceduralStaticInstanceEntry)> creations);
       
        void OnInstancesDeleted(IReadOnlyList<int> instanceIndices);
        
        void OnInstancesDeleteResolve(IReadOnlyList<int> instanceIndices);
        void OnInstanceDataChanged(int instanceIndex, InstanceDataKey key, IDecodableData data, byte context);
        void OnInstanceDataResolved(int instanceIndex, InstanceDataKey key, IDecodableData data, byte context);
        
        void OnInstanceDataInitialized(int instanceIndex, InstanceDataKey key, Func<IPacker> dataFunc);
        
        void OnDynamicObjectsLoaded(IReadOnlyList<(int, DynamicInstanceEntry)> loaded);
        void OnDynamicObjectsCreated(IReadOnlyList<(int, DynamicInstanceEntry)> creations);
        void OnDynamicObjectEntityRequested(IReadOnlyList<(int, string, TransformData)> entityRequests);
        
        void OnDynamicObjectEntityDeleteRequested(IReadOnlyList<int> entityDeleteRequests);

        List<int> GetCurrentNonProceduralDynamicEntities();
    }
}