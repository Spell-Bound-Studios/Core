// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using Spellbound.Core.Packing;

namespace Spellbound.Core {
    public interface IObjectInstanceConsumer {
        void OnRuntimeInstancesLoaded(IReadOnlyList<(int, NonProceduralStaticInstanceEntry)> creations);

        void OnRuntimeInstancesCreated(IReadOnlyList<(int, NonProceduralStaticInstanceEntry)> creations);
        void OnInstancesDeleted(IReadOnlyList<int> instanceIndices);
        void OnInstanceDataChanged(int instanceIndex, InstanceDataKey key, IDecodableData data);
        
    }
}