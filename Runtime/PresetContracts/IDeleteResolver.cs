// Copyright 2026 Spellbound Studio Inc.

using System.Collections.Generic;
using Spellbound.Core.ObjectData;
using Spellbound.Core.ObjectHandling;

namespace Spellbound.Core.PresetContracts {
    public interface IDeleteResolver {
        /// <summary>
        /// For ObjectPreset PresetModules
        /// Interface Contract for a Module to respond to changes with major consequences
        /// </summary>
        void ResolveDelete(Dictionary<InstanceDataKey, byte[]> dataSlots, ObjectParent parent, int instanceIndex, TransformData transformData);
    }
}