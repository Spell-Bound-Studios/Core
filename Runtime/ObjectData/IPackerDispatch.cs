// Copyright 2026 Spellbound Studio Inc.

using System;
using Spellbound.Core.Packing;

namespace Spellbound.Core.ObjectData {
    public interface IPackerDispatch : ISmartPacker {
        static IPackerDispatch SmartUnpackDispatchData(ref ReadOnlySpan<byte> buffer) {
            var result = SmartUnpack(ref buffer);
    
            if (result is not IPackerDispatch dispatchData)
                throw new Exception($"PackerRegistry: '{result.PackerId}' is not an IPackerDispatch.");
    
            return dispatchData;
        }
        
        static IPackerDispatch SmartUnpackDispatchData(byte[] data) {
            ReadOnlySpan<byte> span = data;
            return IPackerDispatch.SmartUnpackDispatchData(ref span);
        }
    }
}