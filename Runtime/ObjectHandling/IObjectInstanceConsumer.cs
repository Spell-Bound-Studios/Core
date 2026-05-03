// Copyright 2026 Spellbound Studio Inc.

using System.Collections.Generic;

namespace Spellbound.Core {
    public interface IObjectInstanceConsumer {
        void OnRuntimeInstancesCreated(IReadOnlyList<RuntimeInstanceCreation> creations);
        void OnInstancesDeleted(IReadOnlyList<int> instanceIndices);
        void OnInstanceDataChanged(int instanceIndex, InstanceDataKey key);
    }
}