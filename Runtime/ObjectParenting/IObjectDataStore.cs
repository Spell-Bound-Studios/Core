// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;

namespace Spellbound.Core {
    /// <summary>
    /// aka "Outer Dictionary"
    /// </summary>
    public interface IObjectDataStore {
        event Action<int> OnInstanceRemoved;

        #region Read

        bool TryGetInstanceBag(int instanceIndex, out InstanceDataBag bag);

        IEnumerable<(int instanceIndex, InstanceDataBag bag)> GetAllBags();

        IEnumerable<(int instanceIndex, InstanceDataBag bag)> GetDirtyBags();

        bool HasInstance(int instanceIndex);

        #endregion

        #region Write

        void WriteInstanceData(int instanceIndex, string packerId, byte[] data);

        #endregion
    }
}