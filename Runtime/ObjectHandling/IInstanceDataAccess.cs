// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Spellbound.Core.ECS;
using Spellbound.Core.Packing;
using UnityEngine;

namespace Spellbound.Core {
    public interface IInstanceDataAccess {

        IObjectParentContext ctx { get; }
        
        void SetContext(IObjectParentContext context);
        
        Dictionary<int, InstanceEntry> GetAllInstances();
        void InstanceStateChange(int instanceIndex, bool isStateFullyDataDriven);
        
        // For for reparenting moving objects
        int MigrateInstance(int instanceIndex, IEventSurface eventSurface, Vector3Int newCoord, IObjectParent newParent);
        
        int AddNewInstanceSilently(InstanceEntry instanceEntry, IEventSurface eventSurface);
        
        void AddInstanceSilently(int instanceIndex, InstanceEntry instanceEntry);
        
        // Updates the transform data of an instance
        void RefreshInstanceTransform(int instanceIndex, TransformData transformData);

        // Intended to be the implementation for a non-procedural object creation via index, presetUid, position, rotation, scale.
        void CreateInstance(string presetUid, Vector3 position, Vector3 rotation, int scale);

        // Intended to be the implementation for a non-procedural object creation via index, presetUid, position, rotation, scale, and additional data.
        void CreateInstanceWithData<T>(
            string presetUid, Vector3 position, Vector3 rotation, int scale, int eventSurfaceIndex, T data)
                where T : IPacker, new();

        // Helper method to see if the index exists.
        bool HasInstance(int instanceIndex);

        // Helper method to see if the index has been deleted.
        bool IsDeleted(int instanceIndex);

        // Intended to be the implementation for simply reading data on an instance.
        bool TryRead<T>(int instanceIndex, int eventSurfaceIndex, out T data)
                where T : IPacker, new();

        // Intended to be the implementation for simply reading data on an instance, guaranteeing a result.
        Task<T> Read<T>(int instanceIndex, string presetUid, int eventSurfaceIndex)
                where T : IPacker, new();

        // Intended to be the implementation for writing over any data with new data on an object.
        void Write<T>(int instanceIndex, string presetUid, int eventSurfaceIndex, T newData)
                where T : IPacker, new();

        // Intended to be the implementation for transforming current object data with incoming data.
        void Delta<T>(int instanceIndex, string presetUid, int eventSurfaceIndex, T delta)
                where T : IQuantitativeData, new();

        // Intended to be the implementation for deleting an instance with confirmation of deletion.
        Task<bool> TryDeleteInstance(int instanceIndex);
    }
}