// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using Spellbound.Core.Packing;

namespace Spellbound.Core {
    public interface IObjectInstanceConsumer {
        void OnRuntimeInstancesCreated(IReadOnlyList<(int, NonProceduralStaticInstanceEntry)> creations);
        void OnInstancesDeleted(IReadOnlyList<int> instanceIndices);
        void OnInstanceDataStructuralChanged<T>(int instanceIndex, InstanceDataKey key, Func<T> dataFunc) where T : IPacker;
        
        void OnInstanceDataCosmeticChanged<T>(int instanceIndex, InstanceDataKey key, Func<T> dataFunc) where T : IPacker;
        
        void OnInstanceDataInitialized<T>(int instanceIndex, InstanceDataKey key, Func<T> dataFunc) where T : IPacker;
    }
}