// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Spellbound.Core.ObjectData;

namespace Spellbound.Core.Packing {
    public static class PackerRegistry {
        private static readonly Dictionary<string, Func<IDecodableData>> Factories = new();

        static PackerRegistry() {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                foreach (var type in assembly.GetTypes()) {
                    if (!typeof(IDecodableData).IsAssignableFrom(type))
                        continue;

                    if (type.IsAbstract || type.IsInterface)
                        continue;

                    var field = type.GetField("ID", BindingFlags.Public | BindingFlags.Static);

                    if (field == null)
                        continue;

                    var id = field.GetValue(null) as string;

                    if (string.IsNullOrEmpty(id))
                        continue;

                    Factories[id] = Expression.Lambda<Func<IDecodableData>>(
                        Expression.Convert(Expression.New(type), typeof(IDecodableData))
                    ).Compile();
                }
            }
        }

        public static bool TryCreateInstance(string packerId, out IDecodableData instance) {
            if (Factories.TryGetValue(packerId, out var factory)) {
                instance = factory();

                return true;
            }

            instance = null;

            return false;
        }

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