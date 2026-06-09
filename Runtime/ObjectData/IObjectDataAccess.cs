// Copyright 2026 Spellbound Studio Inc.

using System.Collections.Generic;
using System.Threading.Tasks;
using Spellbound.Core.ObjectHandling;
using UnityEngine;

namespace Spellbound.Core.ObjectData {
    public interface IObjectDataAccess {
        void SetConsumer(IObjectInstanceConsumer consumer);

        #region Bulk Access

        Dictionary<int, NonProceduralStaticInstanceEntry> GetAllRuntimeInstances();
        IReadOnlyCollection<int> GetAllSeedInstanceDeletions();

        #endregion Bulk Access

        #region Runtime Instance Creation

        void CreateRuntimeInstance(string presetUid, Vector3 position, Vector3 rotation, int scale);

        #endregion

        // Helper method to see if the index exists.
        bool HasInstance(int instanceIndex);

        // Helper method to see if the index has been deleted.
        bool IsDeleted(int instanceIndex);

        // Intended to be the implementation for simply reading data on an instance.
        bool TryRead<T>(int instanceIndex, int eventSurfaceIndex, out T data)
                where T : IPackerObjectData, new();

        bool TryReadAll(int instanceIndex, int eventSurfaceIndex, out List<IPackerObjectData> data);

        // Intended to be the implementation for writing over any data with new data on an object.
        void Write<T>(int instanceIndex, string presetUid, int eventSurfaceIndex, T newData, byte contextIn)
                where T : IPackerObjectData, new();

        // Intended to be the implementation for transforming current object data with incoming data.
        void Delta<TData, TDispatch>(int instanceIndex, string presetUid, int eventSurfaceIndex, TData data, TDispatch dispatch)
                where TData : IPackerObjectData, new() where TDispatch : IPackerDispatch, new();

        // Intended to be the implementation for deleting an instance with confirmation of deletion.
        Task<bool> TryDeleteInstance(int instanceIndex);
    }
}