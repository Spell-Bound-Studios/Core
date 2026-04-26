// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Spellbound.Core.Packing {
    public static class PackerRegistry {
        private static readonly Dictionary<string, Type> IDToType = new();
        private static readonly Dictionary<Type, string> TypeToId = new();
        private static readonly Dictionary<string, Type> IDToHandlerType = new();
        private static readonly Dictionary<string, Func<IPacker>> Factories = new();

        static PackerRegistry() {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                foreach (var type in assembly.GetTypes()) {
                    var attr = type.GetCustomAttribute<PackerIdAttribute>();

                    if (attr == null)
                        continue;

                    IDToType[attr.Id] = type;
                    TypeToId[type] = attr.Id;

                    var handlerAttr = type.GetCustomAttribute<FromHandlerAttribute>();

                    if (handlerAttr != null)
                        IDToHandlerType[attr.Id] = handlerAttr.HandlerType;

                    if (typeof(IPacker).IsAssignableFrom(type)) {
                        Factories[attr.Id] = Expression.Lambda<Func<IPacker>>(
                            Expression.Convert(Expression.New(type), typeof(IPacker))
                        ).Compile();
                    }
                }
            }
        }

        public static bool TryGetType(string id, out Type type) => IDToType.TryGetValue(id, out type);

        public static bool TryGetId(Type type, out string id) => TypeToId.TryGetValue(type, out id);

        public static bool TryGetHandlerType(string packerId, out Type handlerType) =>
                IDToHandlerType.TryGetValue(packerId, out handlerType);

        public static bool TryCreateInstance(string packerId, out IPacker instance) {
            if (Factories.TryGetValue(packerId, out var factory)) {
                instance = factory();

                return true;
            }

            instance = null;

            return false;
        }

        /// <summary>
        /// Provides a readable string based on what's inside a byte[] if we have the packerId.
        /// </summary>
        /// <remarks>
        /// This is useful if you're debugging, logging, or creating an editor tool and don't want to see byte[] in
        /// the inspector.
        /// </remarks>
        public static string Decode(string packerId, byte[] data) {
            if (data == null || data.Length == 0)
                return $"{packerId}: [empty]";

            if (!TryCreateInstance(packerId, out var instance))
                return $"{packerId}: [{data.Length} bytes]";

            try {
                ReadOnlySpan<byte> span = data;
                instance.Unpack(ref span);

                return $"{packerId}: {instance}";
            }
            catch {
                return $"{packerId}: [{data.Length} bytes, decode failed]";
            }
        }
    }
}