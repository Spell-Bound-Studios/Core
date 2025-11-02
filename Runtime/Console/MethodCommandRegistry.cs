// Copyright 2025 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Spellbound.Core.Console {
    /// <summary>
    /// Discovers and stores all methods marked with [ConsoleCommand].
    /// Enables routing console commands to the appropriate game system methods.
    /// </summary>
    public static class MethodCommandRegistry {
        /// <summary>
        /// Registered command handlers indexed by (commandName, moduleType).
        /// </summary>
        private static readonly Dictionary<(string commandName, Type moduleType), MethodInfo> MethodHandlers = new();

        /// <summary>
        /// Cached object instances that own the registered methods.
        /// </summary>
        private static readonly Dictionary<MethodInfo, object> MethodInstances = new();

        private static bool _isInitialized;

        /// <summary>
        /// Initializes the method registry by scanning all assemblies for [ConsoleCommand] methods.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize() {
            if (_isInitialized)
                return;

            DiscoverAllMethods();
            _isInitialized = true;
        }

        /// <summary>
        /// Scans all loaded assemblies for methods with the [ConsoleCommand] attribute.
        /// </summary>
        private static void DiscoverAllMethods() {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var registeredCount = 0;

            foreach (var assembly in assemblies) {
                // Skip editor assemblies and third-party plugins
                var assemblyName = assembly.GetName().Name;
                if (ShouldSkipAssembly(assemblyName))
                    continue;

                try {
                    var types = assembly.GetTypes();

                    foreach (var type in types) {
                        var methods = type
                                .GetMethods(BindingFlags.Public | BindingFlags.NonPublic |
                                                      BindingFlags.Instance | BindingFlags.Static);

                        foreach (var method in methods) {
                            var attribute = method.GetCustomAttribute<ConsoleCommandMethodAttribute>();

                            if (attribute == null)
                                continue;

                            RegisterMethod(method, attribute);
                            registeredCount++;
                        }
                    }
                }
                catch (Exception ex) {
                    Debug.LogWarning(
                        $"[MethodCommandRegistry] Failed to scan assembly {assembly.FullName}: {ex.Message}");
                }
            }

            Debug.Log($"[MethodCommandRegistry] Registered {registeredCount} console command methods");
        }

        /// <summary>
        /// Determines if an assembly should be skipped during discovery.
        /// </summary>
        private static bool ShouldSkipAssembly(string assemblyName) {
            // Skip Unity editor assemblies...
            if (assemblyName.StartsWith("UnityEditor"))
                return true;

            // Skip third-party editor plugins...
            return assemblyName.Contains("Editor") && 
                   (assemblyName.StartsWith("JetBrains") || 
                    assemblyName.StartsWith("Unity.") ||
                    assemblyName.Contains(".Editor."));
        }

        /// <summary>
        /// Registers a single method as a command handler.
        /// </summary>
        private static void RegisterMethod(MethodInfo method, ConsoleCommandMethodAttribute attribute) {
            var key = (attribute.CommandName.ToLower(), attribute.RequiredModuleType);

            if (!MethodHandlers.TryAdd(key, method)) {
                Debug.LogWarning(
                    $"[MethodCommandRegistry] Duplicate handler for command '{attribute.CommandName}' " +
                    $"with module {attribute.RequiredModuleType.Name}");

                return;
            }

            Debug.Log(
                $"[MethodCommandRegistry] Registered {method.DeclaringType?.Name}.{method.Name} " +
                $"for '{attribute.CommandName}' (requires {attribute.RequiredModuleType.Name})");
        }

        /// <summary>
        /// Attempts to find a method handler for the given command and module type.
        /// </summary>
        public static bool TryGetMethodHandler(string commandName, Type moduleType, out MethodInfo method) {
            if (!_isInitialized)
                Initialize();

            var key = (commandName.ToLower(), moduleType);

            return MethodHandlers.TryGetValue(key, out method);
        }

        /// <summary>
        /// Gets or creates an instance of the object that owns the method.
        /// For static methods, returns null.
        /// For instance methods, finds or creates a MonoBehaviour instance.
        /// </summary>
        public static object GetMethodInstance(MethodInfo method) {
            if (method.IsStatic)
                return null;

            // Check cache
            if (MethodInstances.TryGetValue(method, out var cached))
                return cached;

            // Try to find an existing instance in a scene.
            if (typeof(MonoBehaviour).IsAssignableFrom(method.DeclaringType)) {
                var instance = UnityEngine.Object.FindFirstObjectByType(method.DeclaringType) as MonoBehaviour;

                if (instance != null) {
                    MethodInstances[method] = instance;

                    return instance;
                }

                Debug.LogError(
                    $"[MethodCommandRegistry] No instance of {method.DeclaringType?.Name} " +
                    $"found in scene for method {method.Name}");

                return null;
            }

            // For non-MonoBehaviour classes, try to create an instance.
            try {
                if (method.DeclaringType != null) {
                    var instance = Activator.CreateInstance(method.DeclaringType);
                    MethodInstances[method] = instance;

                    return instance;
                }
            }
            catch (Exception ex) {
                Debug.LogError(
                    $"[MethodCommandRegistry] Failed to create instance of {method.DeclaringType?.Name}: {ex.Message}");

                return null;
            }

            return null;
        }

        /// <summary>
        /// Clears all registered methods (for testing purposes).
        /// </summary>
        public static void Clear() {
            MethodHandlers.Clear();
            MethodInstances.Clear();
            _isInitialized = false;
        }
    }
}