// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using Spellbound.Core.Packing;

namespace Spellbound.Core {
    /// <summary>
    /// aka "Outer Dictionary"
    /// </summary>
    public interface IObjectDataStore {
        event Action<int> OnInstanceRemoved;

        #region Read

        bool TryGetInstanceBag(int instanceIndex, out InstanceDataBag bag);

        InstanceDataBag CreateInstanceDataBag<T>(int instanceIndex, string presetuid, T data) where T : IPacker;
        IEnumerable<(int instanceIndex, InstanceDataBag bag)> GetAllBags();

        IEnumerable<(int instanceIndex, InstanceDataBag bag)> GetDirtyBags();

        bool HasInstance(int instanceIndex);

        #endregion

        #region Write

        void WriteInstanceData<T>(int instanceIndex, T data) where T : IPacker;

        #endregion
    }
}