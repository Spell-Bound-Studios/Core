// Copyright 2025 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Spellbound.Core.Console {
    /// <summary>
    /// Discovers and stores all methods marked with [ConsolePresetCommand].
    /// Enables routing console commands to the appropriate game system methods.
    /// </summary>
    public static class PresetCommandRegistry {
        /// <summary>
        /// Preset command handlers indexed by (commandName, moduleType).
        /// </summary>
        private static readonly Dictionary<(string commandName, Type moduleType), MethodInfo> PresetHandlers = new();
        
        /// <summary>
        /// Utility command handlers indexed by commandName only.
        /// </summary>
        private static readonly Dictionary<string, MethodInfo> UtilityHandlers = new();
        
        /// <summary>
        /// All registered command names (for duplicate detection across all types).
        /// </summary>
        private static readonly HashSet<string> AllCommandNames = new();

        /// <summary>
        /// Cached object instances that own the registered methods.
        /// </summary>
        private static readonly Dictionary<MethodInfo, object> MethodInstances = new();

        private static bool _isInitialized;

        /// <summary>
        /// Initializes the method registry by scanning all assemblies for [ConsolePresetCommand] methods.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize() {
            if (_isInitialized)
                return;

            DiscoverAllMethods();
            _isInitialized = true;
        }

        /// <summary>
        /// Scans all loaded assemblies for methods with the [ConsolePresetCommand] attribute.
        /// </summary>
        private static void DiscoverAllMethods() {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var presetCount = 0;
            var utilityCount = 0;

            foreach (var assembly in assemblies) {
                var assemblyName = assembly.GetName().Name;
                if (ShouldSkipAssembly(assemblyName))
                    continue;

                try {
                    var types = assembly.GetTypes();

                    foreach (var type in types) {
                        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic |
                                                      BindingFlags.Instance | BindingFlags.Static);

                        foreach (var method in methods) {
                            // Check for preset command
                            var presetAttr = method.GetCustomAttribute<ConsolePresetCommandAttribute>();
                            if (presetAttr != null) {
                                if (RegisterPresetCommand(method, presetAttr))
                                    presetCount++;
                                continue;
                            }

                            // Check for utility command
                            var utilityAttr = method.GetCustomAttribute<ConsoleUtilityCommandAttribute>();

                            if (utilityAttr == null) 
                                continue;

                            if (RegisterUtilityCommand(method, utilityAttr))
                                utilityCount++;
                        }
                    }
                }
                catch (Exception ex) {
                    Debug.LogWarning($"[PresetCommandRegistry] Failed to scan assembly {assembly.FullName}: {ex.Message}");
                }
            }

            Debug.Log($"[PresetCommandRegistry] Registered {presetCount} preset commands and {utilityCount} utility commands");
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

        private static bool RegisterPresetCommand(MethodInfo method, ConsolePresetCommandAttribute attr) {
            var commandName = attr.CommandName.ToLower();

            // Check for duplicates
            if (AllCommandNames.Contains(commandName)) {
                Debug.LogError($"[PresetCommandRegistry] Duplicate command name '{commandName}' - skipping {method.DeclaringType?.Name}.{method.Name}");
                return false;
            }

            var key = (commandName, attr.RequiredModuleType);

            if (!PresetHandlers.TryAdd(key, method)) {
                Debug.LogWarning($"[PresetCommandRegistry] Duplicate preset handler for '{commandName}' with module {attr.RequiredModuleType.Name}");
                return false;
            }

            AllCommandNames.Add(commandName);
            Debug.Log($"[PresetCommandRegistry] Registered preset command '{commandName}' → {method.DeclaringType?.Name}.{method.Name}");
            return true;
        }
        
        private static bool RegisterUtilityCommand(MethodInfo method, ConsoleUtilityCommandAttribute attr) {
            var commandName = attr.CommandName.ToLower();

            // Must be static
            if (!method.IsStatic) {
                Debug.LogError($"[PresetCommandRegistry] Utility command {method.DeclaringType?.Name}.{method.Name} must be static - skipping");
                return false;
            }

            // Check for duplicates
            if (AllCommandNames.Contains(commandName)) {
                Debug.LogError($"[PresetCommandRegistry] Duplicate command name '{commandName}' - skipping {method.DeclaringType?.Name}.{method.Name}");
                return false;
            }

            if (!UtilityHandlers.TryAdd(commandName, method)) {
                Debug.LogWarning($"[PresetCommandRegistry] Duplicate utility handler for '{commandName}'");
                return false;
            }

            AllCommandNames.Add(commandName);
            Debug.Log($"[PresetCommandRegistry] Registered utility command '{commandName}' → {method.DeclaringType?.Name}.{method.Name}");
            return true;
        }
        
        #region Preset Command API (existing)

        /// <summary>
        /// Attempts to find a preset command handler for the given command and module type.
        /// </summary>
        public static bool TryGetPresetHandler(string commandName, Type moduleType, out MethodInfo method) {
            if (!_isInitialized)
                Initialize();

            var key = (commandName.ToLower(), moduleType);
            return PresetHandlers.TryGetValue(key, out method);
        }

        #endregion
        
        #region Utility Command API (new)

        /// <summary>
        /// Check if a utility command is registered.
        /// </summary>
        public static bool HasUtilityCommand(string commandName) {
            if (!_isInitialized)
                Initialize();

            return UtilityHandlers.ContainsKey(commandName.ToLower());
        }

        /// <summary>
        /// Execute a utility command with the given arguments.
        /// </summary>
        public static CommandResult ExecuteUtilityCommand(string commandName, string[] args) {
            if (!_isInitialized)
                Initialize();

            if (!UtilityHandlers.TryGetValue(commandName.ToLower(), out var method)) {
                return CommandResult.Fail($"Unknown utility command: {commandName}");
            }

            try {
                var parsedArgs = ParseArguments(method, args);
                if (parsedArgs == null) {
                    return CommandResult.Fail(GetUsageString(method));
                }

                // Invoke (static method, so instance is null)
                var result = method.Invoke(null, parsedArgs);

                if (result != null) {
                    return CommandResult.Ok(result.ToString());
                }

                return CommandResult.Ok($"Executed: {commandName}");
            }
            catch (Exception ex) {
                var innerEx = ex.InnerException ?? ex;
                return CommandResult.Fail($"Error executing {commandName}: {innerEx.Message}");
            }
        }

        #endregion

        #region Shared Execution Logic

        /// <summary>
        /// Parse string arguments into typed parameters for a method.
        /// Shared by both preset and utility commands.
        /// </summary>
        private static object[] ParseArguments(MethodInfo method, string[] args) {
            var parameters = method.GetParameters();

            if (parameters.Length == 0) {
                return Array.Empty<object>();
            }

            var parsedArgs = new List<object>();
            var argIndex = 0;

            foreach (var param in parameters) {
                if (argIndex >= args.Length) {
                    // Check for default value
                    if (param.HasDefaultValue) {
                        parsedArgs.Add(param.DefaultValue);
                        continue;
                    }

                    ConsoleLogger.PrintError($"Missing required parameter: {param.Name}");
                    return null;
                }

                // Special handling for Vector3
                if (param.ParameterType == typeof(Vector3)) {
                    if (CommandParameterParser.TryParseVector3(args, argIndex, out var vec3)) {
                        parsedArgs.Add(vec3);
                        argIndex += 3;
                        continue;
                    }

                    ConsoleLogger.PrintError($"Invalid Vector3 for parameter {param.Name}. Expected: x y z");
                    return null;
                }

                // Special handling for Vector2
                if (param.ParameterType == typeof(Vector2)) {
                    if (CommandParameterParser.TryParseVector2(args, argIndex, out var vec2)) {
                        parsedArgs.Add(vec2);
                        argIndex += 2;
                        continue;
                    }

                    ConsoleLogger.PrintError($"Invalid Vector2 for parameter {param.Name}. Expected: x y");
                    return null;
                }

                // Handle List<T>
                if (param.ParameterType.IsGenericType &&
                    param.ParameterType.GetGenericTypeDefinition() == typeof(List<>)) {
                    var elementType = param.ParameterType.GetGenericArguments()[0];
                    var listType = typeof(List<>).MakeGenericType(elementType);
                    var list = Activator.CreateInstance(listType);
                    var addMethod = listType.GetMethod("Add");

                    while (argIndex < args.Length) {
                        if (CommandParameterParser.TryParse(args[argIndex], elementType, out var element)) {
                            addMethod?.Invoke(list, new[] { element });
                            argIndex++;
                        }
                        else {
                            ConsoleLogger.PrintError($"Invalid {elementType.Name} in list: {args[argIndex]}");
                            return null;
                        }
                    }

                    parsedArgs.Add(list);
                    continue;
                }

                // Standard parameter parsing
                if (CommandParameterParser.TryParse(args[argIndex], param.ParameterType, out var parsed)) {
                    parsedArgs.Add(parsed);
                    argIndex++;
                    continue;
                }

                ConsoleLogger.PrintError($"Invalid {CommandParameterParser.GetTypeName(param.ParameterType)} for parameter {param.Name}: {args[argIndex]}");
                return null;
            }

            return parsedArgs.ToArray();
        }

        private static string GetUsageString(MethodInfo method) {
            var commandName = method.GetCustomAttribute<ConsoleUtilityCommandAttribute>()?.CommandName ?? method.Name;
            var usage = $"Usage: {commandName}";

            var parameters = method.GetParameters();
            foreach (var param in parameters) {
                var typeName = CommandParameterParser.GetTypeName(param.ParameterType);

                if (param.ParameterType == typeof(Vector3)) {
                    usage += $" <{param.Name}_x> <{param.Name}_y> <{param.Name}_z>";
                }
                else if (param.ParameterType == typeof(Vector2)) {
                    usage += $" <{param.Name}_x> <{param.Name}_y>";
                }
                else if (param.HasDefaultValue) {
                    usage += $" [{param.Name}:{typeName}]";
                }
                else {
                    usage += $" <{param.Name}:{typeName}>";
                }
            }

            return usage;
        }

        /// <summary>
        /// Gets or creates an instance for non-static methods.
        /// Used by ConsoleCommandRouter for preset commands.
        /// </summary>
        public static object GetMethodInstance(MethodInfo method) {
            if (method.IsStatic)
                return null;

            if (MethodInstances.TryGetValue(method, out var cached))
                return cached;

            if (typeof(MonoBehaviour).IsAssignableFrom(method.DeclaringType)) {
                var instance = UnityEngine.Object.FindFirstObjectByType(method.DeclaringType) as MonoBehaviour;

                if (instance != null) {
                    MethodInstances[method] = instance;
                    return instance;
                }

                Debug.LogError($"[PresetCommandRegistry] No instance of {method.DeclaringType?.Name} found in scene for method {method.Name}");
                return null;
            }

            try {
                if (method.DeclaringType != null) {
                    var instance = Activator.CreateInstance(method.DeclaringType);
                    MethodInstances[method] = instance;
                    return instance;
                }
            }
            catch (Exception ex) {
                Debug.LogError($"[PresetCommandRegistry] Failed to create instance of {method.DeclaringType?.Name}: {ex.Message}");
            }

            return null;
        }

        #endregion

        /// <summary>
        /// Clears all registered methods.
        /// </summary>
        public static void Clear() {
            PresetHandlers.Clear();
            UtilityHandlers.Clear();
            AllCommandNames.Clear();
            MethodInstances.Clear();
            _isInitialized = false;
        }
    }
}