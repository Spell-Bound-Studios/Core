// Copyright 2025 Spellbound Studio Inc.

using System;
using System.Linq;
using UnityEngine;

namespace Spellbound.Core.Console {
    /// <summary>
    /// Routes console commands to the appropriate handler methods.
    /// Orchestrates preset resolution, module checking, and method invocation.
    /// </summary>
    public static class ConsoleCommandRouter {
        /// <summary>
        /// Executes a console command by routing it to the appropriate handler.
        /// </summary>
        public static CommandResult RouteCommand(string commandName, string targetName, string[] args) {
            if (!PresetConsoleRegistry.TryResolvePresetUid(targetName, out var presetUid))
                return CommandResult.Fail($"Unknown target: '{targetName}'");
            
            var preset = presetUid.ResolvePreset();

            if (preset == null) 
                return CommandResult.Fail($"Failed to resolve preset: '{targetName}'");
            
            var moduleTypes = preset.modules
                    .Where(m => m != null)
                    .Select(m => m.GetType())
                    .ToList();

            foreach (var moduleType in moduleTypes) {
                if (MethodCommandRegistry.TryGetMethodHandler(commandName, moduleType, out var method)) {
                    return InvokeMethod(method, preset, presetUid, args);
                }
            }

            return CommandResult.Fail($"No handler found for '{commandName}' with preset '{targetName}'");
        }

        /// <summary>
        /// Invokes the discovered method with appropriate arguments.
        /// </summary>
        private static CommandResult InvokeMethod(
            System.Reflection.MethodInfo method, ObjectPreset preset, string presetUid, string[] args) {
            try {
                var instance = MethodCommandRegistry.GetMethodInstance(method);
                
                preset.TryGetModule<ConsoleModule>(out var consoleModule);
                
                var methodParams = method.GetParameters();
                var invokeArgs = new object[methodParams.Length];

                // TODO: Smart argument mapping based on parameter types
                // For now, assume:
                // (string presetUid, Vector3 position, Quaternion rotation, float scale, SbbData[] data)
                for (var i = 0; i < methodParams.Length; i++) {
                    var param = methodParams[i];

                    if (param.ParameterType == typeof(string) && param.Name == "presetUid")
                        invokeArgs[i] = presetUid;
                    else if (param.ParameterType == typeof(Vector3))
                        invokeArgs[i] = GetExecutionPosition(consoleModule);
                    else if (param.ParameterType == typeof(Quaternion))
                        invokeArgs[i] = Quaternion.identity;
                    else if (param.ParameterType == typeof(float))
                        invokeArgs[i] = 1f; // Default scale
                    else if (param.ParameterType == typeof(int)) {
                        // Try to parse quantity from args
                        if (args.Length > 0 && int.TryParse(args[0], out var qty))
                            invokeArgs[i] = qty;
                        else
                            invokeArgs[i] = consoleModule?.defaultQuantity ?? 1;
                    }
                    else if (param.ParameterType.IsArray) {
                        // Default to null/empty for arrays
                        invokeArgs[i] = null;
                    }
                    else
                        invokeArgs[i] = param.HasDefaultValue ? param.DefaultValue : null;
                }
                
                method.Invoke(instance, invokeArgs);

                return CommandResult.Ok($"Executed {method.Name} for {preset.objectName}");
            }
            catch (Exception ex) {
                Debug.LogError($"[ConsoleCommandRouter] Failed to invoke {method.Name}: {ex.Message}");

                return CommandResult.Fail($"Execution failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the world position where the command should be executed.
        /// </summary>
        private static Vector3 GetExecutionPosition(ConsoleModule module) {
            if (module == null)
                return GetCrosshairPosition();

            return module.spawnLocation switch {
                SpawnLocation.AtCrosshair => GetCrosshairPosition(),
                _ => Vector3.zero
            };
        }

        /// <summary>
        /// Raycasts from the camera center to get crosshair world position.
        /// </summary>
        private static Vector3 GetCrosshairPosition() {
            var camera = Camera.main;

            if (camera == null)
                return Vector3.zero;

            var ray = camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

            if (Physics.Raycast(ray, out var hit, 1000f))
                return hit.point;

            return camera.transform.position + camera.transform.forward * 5f;
        }
    }
}