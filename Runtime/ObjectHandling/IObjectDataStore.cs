// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Threading.Tasks;
using Spellbound.Core.Packing;
using Unity.Mathematics;
using UnityEngine;

namespace Spellbound.Core {
    public interface IObjectDataStore {
        event Action<int, string, Vector3, Vector3, int> OnInstanceCreated;
        event Action<int> OnInstanceRemoved;
        event Action<int, (string, int)> OnInstanceDataChanged; // instanceIdx, packerId
        
        int NextInstanceIndex { get; set; }

        void StoreInstance(int instanceIndex, string presetUid);
        
        Task<int> CreateInstance(string presetUid, Vector3 position, Vector3 rotation, int scale);
        
        void CreateInstanceWithData<T>(string presetUid, Vector3 position, Vector3 rotation, int scale, int eventSurfaceIndex, T data)
                where T : IPacker, new();
        
        bool HasInstance(int instanceIndex);
        string GetPresetUid(int instanceIndex);
        
        bool TryRead<T>(int instanceIndex, int eventSurfaceIndex, out T data)
                where T : IPacker, new();
        Task<T> Read<T>(int instanceIndex,string presetUid, int eventSurfaceIndex)
                where T : IPacker, new();
        void Write<T>(int instanceIndex, string presetUid, int eventSurfaceIndex, T newData)
                where T : IPacker, new(); 

        void Delta<T>(int instanceIndex, string presetUid, int eventSurfaceIndex, T delta)
                where T : IQuantitativeData, new();
        Task<bool> DeleteInstance(int instanceIndex);
    }
}