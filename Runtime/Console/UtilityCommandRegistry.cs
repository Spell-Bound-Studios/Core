// Copyright 2025 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using UnityEngine;

namespace Spellbound.Core.Console {
    /// <summary>
    /// Discovers and executes methods marked with [ConsoleUtilityCommand].
    /// Handles parameter parsing and method invocation for non-preset commands.
    /// </summary>
    public static class UtilityCommandRegistry {
        private class UtilityCommandInfo {
            public MethodInfo Method;
            public string CommandName;
            public string Description;
            public string[] Aliases;
            public ParameterInfo[] Parameters;
        }

        private static readonly Dictionary<string, UtilityCommandInfo> Commands = new();
        private static bool _isInitialized;

        /// <summary>
        /// Initializes the utility command registry by scanning for [ConsoleUtilityCommand] methods.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize() {
            if (_isInitialized)
                return;

            DiscoverAllCommands();
            _isInitialized = true;
        }

        /// <summary>
        /// Scans all assemblies for methods with [ConsoleUtilityCommand] attribute.
        /// </summary>
        private static void DiscoverAllCommands() {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var registeredCount = 0;

            foreach (var assembly in assemblies) {
                var assemblyName = assembly.GetName().Name;
                if (ShouldSkipAssembly(assemblyName))
                    continue;

                try {
                    var types = assembly.GetTypes();

                    foreach (var type in types) {
                        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static);

                        foreach (var method in methods) {
                            var attr = method.GetCustomAttribute<ConsoleUtilityCommandAttribute>();
                            if (attr == null)
                                continue;

                            RegisterCommand(method, attr);
                            registeredCount++;
                        }
                    }
                }
                catch (Exception ex) {
                    Debug.LogWarning($"[UtilityCommandRegistry] Error scanning assembly {assembly.FullName}: {ex.Message}");
                }
            }

            Debug.Log($"[UtilityCommandRegistry] Registered {registeredCount} utility commands");
        }

        private static bool ShouldSkipAssembly(string assemblyName) {
            if (assemblyName.StartsWith("UnityEditor"))
                return true;

            return assemblyName.Contains("Editor") &&
                   (assemblyName.StartsWith("JetBrains") ||
                    assemblyName.StartsWith("Unity.") ||
                    assemblyName.Contains(".Editor."));
        }

        private static void RegisterCommand(MethodInfo method, ConsoleUtilityCommandAttribute attr) {
            // Must be static
            if (!method.IsStatic) {
                Debug.LogWarning($"[UtilityCommandRegistry] Method {method.DeclaringType?.Name}.{method.Name} must be static");
                return;
            }

            var commandInfo = new UtilityCommandInfo {
                Method = method,
                CommandName = attr.CommandName.ToLower(),
                Description = attr.Description,
                Aliases = attr.Aliases,
                Parameters = method.GetParameters()
            };

            // Register main command name
            if (!Commands.TryAdd(commandInfo.CommandName, commandInfo)) {
                Debug.LogWarning($"[UtilityCommandRegistry] Duplicate command: {commandInfo.CommandName}");
                return;
            }

            // Register aliases
            foreach (var alias in attr.Aliases) {
                Commands.TryAdd(alias.ToLower(), commandInfo);
            }

            Debug.Log($"[UtilityCommandRegistry] Registered '{commandInfo.CommandName}' → {method.DeclaringType?.Name}.{method.Name}");
        }

        /// <summary>
        /// Check if a command is registered.
        /// </summary>
        public static bool HasCommand(string commandName) {
            if (!_isInitialized)
                Initialize();

            return Commands.ContainsKey(commandName.ToLower());
        }

        /// <summary>
        /// Execute a utility command with the given arguments.
        /// </summary>
        public static CommandResult ExecuteCommand(string commandName, string[] args) {
            if (!_isInitialized)
                Initialize();

            if (!Commands.TryGetValue(commandName.ToLower(), out var commandInfo)) {
                return CommandResult.Fail($"Unknown utility command: {commandName}");
            }

            try {
                // Parse arguments
                var parsedArgs = ParseArguments(commandInfo, args);
                if (parsedArgs == null) {
                    return CommandResult.Fail(GetUsageString(commandInfo));
                }

                // Invoke method (static, so instance is null)
                var result = commandInfo.Method.Invoke(null, parsedArgs);

                // Return result
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

        private static object[] ParseArguments(UtilityCommandInfo commandInfo, string[] args) {
            var parameters = commandInfo.Parameters;

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

                // Special handling for Vector3 (consumes 3 args)
                if (param.ParameterType == typeof(Vector3)) {
                    if (TryParseVector3(args, argIndex, out var vec3)) {
                        parsedArgs.Add(vec3);
                        argIndex += 3;
                        continue;
                    }

                    ConsoleLogger.PrintError($"Invalid Vector3 for parameter {param.Name}. Expected: x y z");
                    return null;
                }

                // Special handling for Vector2 (consumes 2 args)
                if (param.ParameterType == typeof(Vector2)) {
                    if (TryParseVector2(args, argIndex, out var vec2)) {
                        parsedArgs.Add(vec2);
                        argIndex += 2;
                        continue;
                    }

                    ConsoleLogger.PrintError($"Invalid Vector2 for parameter {param.Name}. Expected: x y");
                    return null;
                }

                // Handle List<T> - collect remaining args
                if (param.ParameterType.IsGenericType &&
                    param.ParameterType.GetGenericTypeDefinition() == typeof(List<>)) {
                    var elementType = param.ParameterType.GetGenericArguments()[0];
                    var listType = typeof(List<>).MakeGenericType(elementType);
                    var list = Activator.CreateInstance(listType);
                    var addMethod = listType.GetMethod("Add");

                    while (argIndex < args.Length) {
                        if (TryParseValue(args[argIndex], elementType, out var element)) {
                            addMethod.Invoke(list, new[] { element });
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
                if (TryParseValue(args[argIndex], param.ParameterType, out var parsed)) {
                    parsedArgs.Add(parsed);
                    argIndex++;
                    continue;
                }

                ConsoleLogger.PrintError($"Invalid {GetTypeName(param.ParameterType)} for parameter {param.Name}: {args[argIndex]}");
                return null;
            }

            return parsedArgs.ToArray();
        }

        private static bool TryParseValue(string arg, Type targetType, out object result) {
            result = null;

            try {
                // Handle nullable types
                if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>)) {
                    targetType = Nullable.GetUnderlyingType(targetType);
                }

                // String
                if (targetType == typeof(string)) {
                    result = arg;
                    return true;
                }

                // Int
                if (targetType == typeof(int)) {
                    if (int.TryParse(arg, out var intVal)) {
                        result = intVal;
                        return true;
                    }
                    return false;
                }

                // Float
                if (targetType == typeof(float)) {
                    if (float.TryParse(arg, NumberStyles.Float, CultureInfo.InvariantCulture, out var floatVal)) {
                        result = floatVal;
                        return true;
                    }
                    return false;
                }

                // Double
                if (targetType == typeof(double)) {
                    if (double.TryParse(arg, NumberStyles.Float, CultureInfo.InvariantCulture, out var doubleVal)) {
                        result = doubleVal;
                        return true;
                    }
                    return false;
                }

                // Bool
                if (targetType == typeof(bool)) {
                    if (bool.TryParse(arg, out var boolVal)) {
                        result = boolVal;
                        return true;
                    }
                    if (arg == "1" || arg.ToLower() == "true") {
                        result = true;
                        return true;
                    }
                    if (arg == "0" || arg.ToLower() == "false") {
                        result = false;
                        return true;
                    }
                    return false;
                }

                // Byte
                if (targetType == typeof(byte)) {
                    if (byte.TryParse(arg, out var byteVal)) {
                        result = byteVal;
                        return true;
                    }
                    return false;
                }

                // Enum
                if (targetType.IsEnum) {
                    result = Enum.Parse(targetType, arg, true);
                    return true;
                }

                return false;
            }
            catch {
                return false;
            }
        }

        private static bool TryParseVector3(string[] args, int startIndex, out Vector3 result) {
            result = Vector3.zero;

            if (startIndex + 2 >= args.Length)
                return false;

            if (!float.TryParse(args[startIndex], NumberStyles.Float, CultureInfo.InvariantCulture, out var x))
                return false;

            if (!float.TryParse(args[startIndex + 1], NumberStyles.Float, CultureInfo.InvariantCulture, out var y))
                return false;

            if (!float.TryParse(args[startIndex + 2], NumberStyles.Float, CultureInfo.InvariantCulture, out var z))
                return false;

            result = new Vector3(x, y, z);
            return true;
        }

        private static bool TryParseVector2(string[] args, int startIndex, out Vector2 result) {
            result = Vector2.zero;

            if (startIndex + 1 >= args.Length)
                return false;

            if (!float.TryParse(args[startIndex], NumberStyles.Float, CultureInfo.InvariantCulture, out var x))
                return false;

            if (!float.TryParse(args[startIndex + 1], NumberStyles.Float, CultureInfo.InvariantCulture, out var y))
                return false;

            result = new Vector2(x, y);
            return true;
        }

        private static string GetUsageString(UtilityCommandInfo commandInfo) {
            var usage = $"Usage: {commandInfo.CommandName}";

            foreach (var param in commandInfo.Parameters) {
                var paramName = param.Name;
                var typeName = GetTypeName(param.ParameterType);

                if (param.ParameterType == typeof(Vector3)) {
                    usage += $" <{paramName}_x> <{paramName}_y> <{paramName}_z>";
                }
                else if (param.ParameterType == typeof(Vector2)) {
                    usage += $" <{paramName}_x> <{paramName}_y>";
                }
                else if (param.HasDefaultValue) {
                    usage += $" [{paramName}:{typeName}={param.DefaultValue}]";
                }
                else {
                    usage += $" <{paramName}:{typeName}>";
                }
            }

            if (!string.IsNullOrEmpty(commandInfo.Description)) {
                usage += $"\n{commandInfo.Description}";
            }

            if (commandInfo.Aliases.Length > 0) {
                usage += $"\nAliases: {string.Join(", ", commandInfo.Aliases)}";
            }

            return usage;
        }

        private static string GetTypeName(Type type) {
            if (type == typeof(int)) return "int";
            if (type == typeof(float)) return "float";
            if (type == typeof(double)) return "double";
            if (type == typeof(bool)) return "bool";
            if (type == typeof(string)) return "string";
            if (type == typeof(byte)) return "byte";
            if (type == typeof(Vector3)) return "Vector3";
            if (type == typeof(Vector2)) return "Vector2";
            if (type.IsEnum) return type.Name;

            return type.Name;
        }

        /// <summary>
        /// Get all registered command names.
        /// </summary>
        public static IEnumerable<string> GetAllCommandNames() {
            if (!_isInitialized)
                Initialize();

            return Commands.Keys;
        }

        /// <summary>
        /// Get help text for a specific command.
        /// </summary>
        public static string GetHelp(string commandName) {
            if (!_isInitialized)
                Initialize();

            if (!Commands.TryGetValue(commandName.ToLower(), out var commandInfo)) {
                return $"Unknown command: {commandName}";
            }

            return GetUsageString(commandInfo);
        }

        /// <summary>
        /// Clear all registered commands (for testing).
        /// </summary>
        public static void Clear() {
            Commands.Clear();
            _isInitialized = false;
        }
    }
}