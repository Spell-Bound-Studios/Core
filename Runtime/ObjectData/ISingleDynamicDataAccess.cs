// Copyright 2026 Spellbound Studio Inc.

namespace Spellbound.Core.ObjectData {
    public interface ISingleDynamicDataAccess {
        bool TryRead<T>(int eventSurfaceIndex, out T data)
                where T : IPackerObjectData, new();
        
        void Write<T>(int eventSurfaceIndex, T newData, byte contextIn)
                where T : IPackerObjectData, new();
        
        void Delta<T>(int eventSurfaceIndex, T delta)
                where T : IPackerObjectData, new();
    }
}