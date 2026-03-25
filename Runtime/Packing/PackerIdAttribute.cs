// Copyright 2026 Spellbound Studio Inc.
 
using System;
using System.Collections.Generic;
using System.Reflection;
 
namespace Spellbound.Core.Packing {
    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
    public sealed class PackerIdAttribute : Attribute {
        public string Id { get; }
        public PackerIdAttribute(string id) => Id = id;
    }
    
    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
    public sealed class FromHandlerAttribute : Attribute {
        public Type HandlerType { get; }
    
        public FromHandlerAttribute(Type handlerType) {
            if (!typeof(IHandlerThreshold).IsAssignableFrom(handlerType))
                throw new ArgumentException($"{handlerType.Name} must implement IHandlerThreshold");
            HandlerType = handlerType;
        }
    }
 
    public static class PackerIdCache<T> {
        public static readonly string Id = typeof(T).GetCustomAttribute<PackerIdAttribute>()?.Id;
    }
 
    public static class PackerRegistry {
        private static readonly Dictionary<string, Type> _idToType = new();
        private static readonly Dictionary<Type, string> _typeToId = new();
 
        static PackerRegistry() {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                foreach (Type type in assembly.GetTypes()) {
                    PackerIdAttribute attr = type.GetCustomAttribute<PackerIdAttribute>();
                    if (attr == null) continue;
 
                    _idToType[attr.Id] = type;
                    _typeToId[type] = attr.Id;
                }
            }
        }
 
        public static bool TryGetType(string id, out Type type) =>
                _idToType.TryGetValue(id, out type);
 
        public static bool TryGetId(Type type, out string id) =>
                _typeToId.TryGetValue(type, out id);
        
        public static bool TryGetHandlerType(string packerId, out Type handlerType) {
            if (!TryGetType(packerId, out Type type)) {
                handlerType = null;
                return false;
            }
            handlerType = type.GetCustomAttribute<FromHandlerAttribute>()?.HandlerType;
            return handlerType != null;
        }
    }
}