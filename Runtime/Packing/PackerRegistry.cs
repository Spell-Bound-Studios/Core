// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Spellbound.Core.Logging;
using Spellbound.Core.ObjectData;

namespace Spellbound.Core.Packing {
    public static class PackerRegistry {
        private static readonly Dictionary<ushort, Func<ISmartPacker>> Factories = new();
        private static readonly Dictionary<string, ushort> HashCache = new();

        static PackerRegistry() {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                foreach (var type in assembly.GetTypes()) {
                    if (!typeof(ISmartPacker).IsAssignableFrom(type))
                        continue;

                    if (type.IsAbstract || type.IsInterface)
                        continue;

                    var field = type.GetField("ID", BindingFlags.Public | BindingFlags.Static);

                    if (field == null)
                        continue;

                    var id = field.GetValue(null) as string;

                    if (string.IsNullOrEmpty(id))
                        continue;

                    var hash = ComputeHash(id);

                    if (Factories.ContainsKey(hash))
                        throw new Exception($"PackerRegistry: hash collision or duplicate PackerId '{id}' (hash: {hash})");

                    var factory = Expression.Lambda<Func<ISmartPacker>>(
                        Expression.Convert(Expression.New(type), typeof(ISmartPacker))
                    ).Compile();

                    Factories[hash] = factory;
                    HashCache[id] = hash;

                    Log.Debug($"Registered '{id}' ({type.Name}) -> hash {hash}");
                }
            }
        }

        public static ushort GetHash(string packerId) {
            if (HashCache.TryGetValue(packerId, out var hash))
                return hash;

            throw new Exception($"PackerRegistry: '{packerId}' is not registered.");
        }

        public static bool TryCreateInstance(ushort hash, out ISmartPacker instance) {
            if (Factories.TryGetValue(hash, out var factory)) {
                instance = factory();
                return true;
            }

            instance = null;

            return false;
        }

        public static string Decode(string packerId, byte[] data) {
            return Decode(GetHash(packerId), data);
        }

        public static string Decode(ushort hash, byte[] data) {
            if (data == null || data.Length == 0)
                return $"hash:{hash} [empty]";

            if (!TryCreateInstance(hash, out var instance))
                return $"hash:{hash} [unregistered, {data.Length} bytes]";

            try {
                ReadOnlySpan<byte> span = data;
                instance.Unpack(ref span);

                return $"hash:{hash} ({instance.PackerId}) {instance}";
            }
            catch {
                return $"hash:{hash} ({instance.PackerId}) [{data.Length} bytes, decode failed]";
            }
        }
        
        private static ushort ComputeHash(string packerId) {
            var hash = 2166136261u;

            foreach (var c in packerId) {
                hash ^= c;
                hash *= 16777619u;
            }

            return (ushort)(hash ^ (hash >> 16));
        }
    }
}