// Copyright 2025 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Spellbound.Core.Console {
    /// <summary>
    /// Shared utilities for command registry implementations.
    /// Provides common functionality for assembly scanning, name normalization, and logging.
    /// </summary>
    internal static class CommandRegistryUtilities {
        /// <summary>
        /// Gets all assemblies that should be scanned for commands.
        /// Filters out Unity editor assemblies and third-party editor plugins.
        /// </summary>
        public static IEnumerable<Assembly> GetScannableAssemblies() {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            return assemblies.Where(assembly => {
                var assemblyName = assembly.GetName().Name;

                return !ShouldSkipAssembly(assemblyName);
            });
        }

        /// <summary>
        /// Determines if an assembly should be skipped during command discovery.
        /// </summary>
        public static bool ShouldSkipAssembly(string assemblyName) {
            if (string.IsNullOrEmpty(assemblyName))
                return true;

            if (assemblyName.StartsWith("UnityEditor"))
                return true;

            return assemblyName.Contains("Editor") &&
                   (assemblyName.StartsWith("JetBrains") ||
                    assemblyName.StartsWith("Unity.") ||
                    assemblyName.Contains(".Editor."));
        }

        /// <summary>
        /// Normalizes a command name to lowercase for consistent lookups.
        /// Returns null if the name is invalid.
        /// </summary>
        public static string NormalizeCommandName(string name) =>
                string.IsNullOrWhiteSpace(name)
                        ? null
                        : name.ToLower();

        /// <summary>
        /// Creates a case-insensitive string dictionary for storing commands.
        /// </summary>
        public static Dictionary<string, TValue> CreateCaseInsensitiveDictionary<TValue>() =>
                new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Logs successful command registration.
        /// </summary>
        public static void LogCommandRegistered(string commandName, string targetInfo, string[] aliases = null) {
            var message = $"Registered command: {commandName} → {targetInfo}";

            if (aliases is { Length: > 0 })
                message += $" (aliases: {string.Join(", ", aliases)})";

            Debug.Log(message);
        }

        /// <summary>
        /// Logs a duplicate command warning.
        /// </summary>
        public static void LogDuplicateCommand(string commandName, string location) =>
                Debug.LogWarning(
                    $"Command '{commandName}' is already registered. Attempted registration from {location} was skipped.");

        /// <summary>
        /// Logs an assembly scanning error.
        /// </summary>
        public static void LogAssemblyScanError(string assemblyName, string errorMessage) =>
                Debug.LogWarning($"Failed to scan assembly {assemblyName}: {errorMessage}");

        /// <summary>
        /// Logs a summary of command discovery results.
        /// </summary>
        public static void LogDiscoverySummary(string registryName, int commandCount) =>
                Debug.Log($"[{registryName}] Registered {commandCount} command(s)");

        /// <summary>
        /// Logs a summary with multiple command types.
        /// </summary>
        public static void LogDiscoverySummary(string registryName, params (string type, int count)[] commandTypes) {
            var parts = commandTypes.Select(ct => $"{ct.count} {ct.type}").ToArray();
            var summary = string.Join(" and ", parts);

            Debug.Log($"[{registryName}] Registered {summary}");
        }

        /// <summary>
        /// Validates that a command name is not null or empty.
        /// Throws ArgumentException if invalid.
        /// </summary>
        public static void ValidateCommandName(string commandName, string parameterName = "commandName") {
            if (string.IsNullOrWhiteSpace(commandName))
                throw new ArgumentException("Command name cannot be null or empty", parameterName);
        }
    }
}