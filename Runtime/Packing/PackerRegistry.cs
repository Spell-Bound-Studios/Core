// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Spellbound.Core.Packing {
    public static class PackerRegistry {
        private static readonly Dictionary<string, Type> _idToType = new();
        private static readonly Dictionary<Type, string> _typeToId = new();
        private static readonly Dictionary<string, Type> _idToHandlerType = new();
        private static readonly Dictionary<string, Func<IQuantitativeData>> _factories = new();

        static PackerRegistry() {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                foreach (Type type in assembly.GetTypes()) {
                    PackerIdAttribute attr = type.GetCustomAttribute<PackerIdAttribute>();
                    if (attr == null) continue;

                    _idToType[attr.Id] = type;
                    _typeToId[type] = attr.Id;

                    FromHandlerAttribute handlerAttr = type.GetCustomAttribute<FromHandlerAttribute>();
                    if (handlerAttr != null)
                        _idToHandlerType[attr.Id] = handlerAttr.HandlerType;

                    if (typeof(IQuantitativeData).IsAssignableFrom(type))
                        _factories[attr.Id] = Expression.Lambda<Func<IQuantitativeData>>(
                            Expression.Convert(Expression.New(type), typeof(IQuantitativeData))  // changed
                        ).Compile();
                }
            }
        }

        public static bool TryGetType(string id, out Type type) =>
                _idToType.TryGetValue(id, out type);

        public static bool TryGetId(Type type, out string id) =>
                _typeToId.TryGetValue(type, out id);

        public static bool TryGetHandlerType(string packerId, out Type handlerType) =>
                _idToHandlerType.TryGetValue(packerId, out handlerType);

        public static bool TryCreateInstance(string packerId, out IQuantitativeData instance) {
            if (_factories.TryGetValue(packerId, out var factory)) {
                instance = factory();
                return true;
            }
            instance = null;
            return false;
        }
    }
}