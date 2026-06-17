// Copyright 2026 Spellbound Studio Inc.

using System.Collections.Generic;

namespace Spellbound.Core.ObjectData {
    public interface ISingleObjectDataAccess {
        bool TryRead<T>(int eventSurfaceIndex, out T data) where T : IPackerObjectData, new();

        void Write<T>(int eventSurfaceIndex, T newData, byte contextIn) where T : IPackerObjectData, new();

        public void Delta<TData, TDispatch>(int eventSurfaceIndex, TDispatch dispatch)
                where TData : IPackerObjectData, new()
                where TDispatch : IPackerDispatch, new();
        
        bool TryReadAllBySurface(int eventSurfaceIndex, out List<IPackerObjectData> data);
        
        bool TryReadAll(out List<IPackerObjectData> allData);
    }
}