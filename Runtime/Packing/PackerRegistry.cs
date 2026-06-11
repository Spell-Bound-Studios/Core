// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Spellbound.Core.Hashing;
using Spellbound.Core.Logging;

namespace Spellbound.Core.Packing {
    /// <summary>
    /// Discovers every concrete <see cref="ISmartPacker"/> via its <see cref="PackerIdAttribute"/> and resolves
    /// type / id / hash in all directions. A concrete ISmartPacker without the attribute, a duplicate id, or a
    /// hash collision throws at startup.
    /// </summary>
    public static class PackerRegistry {
        private static readonly Dictionary<uint, Func<ISmartPacker>> Factories = new();
        private static readonly Dictionary<Type, string> IdsByType = new();
        private static readonly Dictionary<Type, uint> HashesByType = new();

        static PackerRegistry() {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                foreach (var type in assembly.GetTypes()) {
                    if (!typeof(ISmartPacker).IsAssignableFrom(type))
                        continue;

                    if (type.IsAbstract || type.IsInterface)
                        continue;

                    var attribute = type.GetCustomAttribute<PackerIdAttribute>();

                    if (attribute == null)
                        throw new Exception(
                            $"PackerRegistry: '{type.FullName}' implements ISmartPacker but has no [PackerId] attribute.");

                    var id = attribute.Id;

                    if (string.IsNullOrEmpty(id))
                        throw new Exception($"PackerRegistry: '{type.FullName}' has an empty [PackerId].");

                    var hash = StableHash.Fnv1A32(id);

                    if (Factories.ContainsKey(hash))
                        throw new Exception(
                            $"PackerRegistry: hash collision or duplicate PackerId '{id}' (hash: {hash})");

                    var factory = Expression.Lambda<Func<ISmartPacker>>(
                        Expression.Convert(Expression.New(type), typeof(ISmartPacker))
                    ).Compile();

                    Factories[hash] = factory;
                    IdsByType[type] = id;
                    HashesByType[type] = hash;

                    Log.Debug($"Registered '{id}' ({type.Name}) -> hash {hash}");
                }
            }
        }

        /// <summary>
        /// The stable hash for a registered packer type, throwing if the type is not registered.
        /// </summary>
        public static uint GetHash(Type type) {
            if (HashesByType.TryGetValue(type, out var hash))
                return hash;

            throw new Exception($"PackerRegistry: '{type.FullName}' is not registered.");
        }

        /// <summary>
        /// The stable hash for a registered packer type. Cached per closed generic type, so after the first
        /// call this is a static field read.
        /// </summary>
        public static uint GetHash<T>() where T : ISmartPacker => Cache<T>.Value;

        private static class Cache<T> where T : ISmartPacker {
            public static readonly uint Value = GetHash(typeof(T));
        }

        /// <summary>
        /// The declared id string for a registered packer type, throwing if the type is not registered.
        /// </summary>
        public static string GetId(Type type) {
            if (IdsByType.TryGetValue(type, out var id))
                return id;

            throw new Exception($"PackerRegistry: '{type.FullName}' is not registered.");
        }

        public static bool TryCreateInstance(uint hash, out ISmartPacker instance) {
            if (Factories.TryGetValue(hash, out var factory)) {
                instance = factory();

                return true;
            }

            instance = null;

            return false;
        }

        public static string Decode(uint hash, byte[] data) {
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
    }
}
