// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Spellbound.Core.Packing;
using UnityEngine;

namespace Spellbound.Core {
    public interface IObjectDataAccess {
        
        // On Loading this is all the Instances to Instantiate.
        Dictionary<int, InstanceEntry> GetAllInstances();
        
        // Intended to flag a non-procedural objects creation via index, presetUid, position, rotation, scale.
        event Action<int, string, TransformData> OnInstanceCreated;

        // Intended to flag any objects deletion via index.
        event Action<int> OnInstanceRemoved;

        // Intended to flag any objects' transformation.
        event Action<int, InstanceDataKey> OnInstanceDataChanged;

        // Intended to separate procedural instances from runtime instances.
        void SetProceduralInstanceCount(int instanceIndex);

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