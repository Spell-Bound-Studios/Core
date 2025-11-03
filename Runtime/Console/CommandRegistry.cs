// Copyright 2025 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Spellbound.Core.Console {
    /// <summary>
    /// Central registry for all console commands. The act of adding the attribute auto registers things here.
    /// </summary>
    public class CommandRegistry {
        private static CommandRegistry _instance;
        public static CommandRegistry Instance => _instance ??= new CommandRegistry();

        // Stores all of our commands that implement the interface ICommand.
        private readonly Dictionary<string, ICommand> _commands = new(StringComparer.OrdinalIgnoreCase);

        // Stores the aliases that get registered via the ConsoleCommandAttribute.
        private readonly Dictionary<string, string> _aliases = new(StringComparer.OrdinalIgnoreCase);

        private CommandRegistry() { }
        public static CommandRegistry CreateInstance() => new();

        /// <summary>
        /// Auto-discover and register all commands with the [ConsoleCommand] attribute.
        /// Call this during initialization.
        /// </summary>
        public void AutoRegisterCommands() {
            // https://learn.microsoft.com/en-us/dotnet/api/system.appdomain.getassemblies?view=net-9.0
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies) {
                try {
                    AutoRegisterCommandsFromAssembly(assembly);
                }
                catch (Exception ex) {
                    Debug.LogWarning($"Failed to load commands from assembly {assembly.FullName}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Auto-register commands from a specific assembly.
        /// </summary>
        public void AutoRegisterCommandsFromAssembly(Assembly assembly) {
            var commandTypes = assembly.GetTypes()
                    .Where(t => t.IsClass && !t.IsAbstract && typeof(ICommand).IsAssignableFrom(t))
                    .Where(t => t.GetCustomAttribute<ConsoleCommandClassAttribute>() != null);

            foreach (var type in commandTypes) {
                try {
                    var attribute = type.GetCustomAttribute<ConsoleCommandClassAttribute>();
                    var command = (ICommand)Activator.CreateInstance(type);

                    Register(command, attribute.Aliases);
                }
                catch (Exception ex) {
                    Debug.LogError($"Failed to register command {type.Name}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Manually register a command instance.
        /// </summary>
        public void Register(ICommand command, params string[] aliases) {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            if (string.IsNullOrWhiteSpace(command.Name))
                throw new ArgumentException("Command name cannot be null or empty");

            var commandName = command.Name.ToLower();

            if (_commands.ContainsKey(commandName))
                Debug.LogWarning($"Command '{commandName}' is already registered. Overwriting.");

            _commands[commandName] = command;

            // Register aliases
            if (aliases != null) {
                foreach (var alias in aliases) {
                    if (!string.IsNullOrWhiteSpace(alias))
                        _aliases[alias.ToLower()] = commandName;
                }
            }

            Debug.Log($"Registered command: {commandName}" +
                      (aliases?.Length > 0 ? $" (aliases: {string.Join(", ", aliases)})" : ""));
        }

        /// <summary>
        /// Unregister a command
        /// </summary>
        public bool Unregister(string commandName) {
            if (string.IsNullOrWhiteSpace(commandName))
                return false;

            commandName = commandName.ToLower();

            // Remove aliases
            var aliasesToRemove = _aliases
                    .Where(kvp => kvp.Value == commandName)
                    .Select(kvp => kvp.Key)
                    .ToList();

            foreach (var alias in aliasesToRemove)
                _aliases.Remove(alias);

            return _commands.Remove(commandName);
        }

        /// <summary>
        /// Try to get a command by name or alias.
        /// </summary>
        public bool TryGetCommand(string name, out ICommand command) {
            if (string.IsNullOrWhiteSpace(name)) {
                command = null;

                return false;
            }

            name = name.ToLower();

            // Check direct command name
            if (_commands.TryGetValue(name, out command))
                return true;

            // Check aliases
            if (_aliases.TryGetValue(name, out var commandName))
                return _commands.TryGetValue(commandName, out command);

            command = null;

            return false;
        }

        /// <summary>
        /// Execute a command - routes to either class-based or method-based handlers.
        /// </summary>
        public CommandResult ExecuteCommand(string input) {
            var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 0)
                return CommandResult.Fail("Empty command");

            var commandName = parts[0];
            var args = parts.Skip(1).ToArray();
            
            if (TryGetCommand(commandName, out var command)) 
                return command.Execute(args);

            // If not, try to route it as a method-based command
            if (args.Length <= 0) 
                return CommandResult.Fail($"Unknown command: '{commandName}'");

            var targetName = args[0];
            var remainingArgs = args.Skip(1).ToArray();

            return ConsoleCommandRouter.RouteCommand(commandName, targetName, remainingArgs);
        }

        /// <summary>
        /// Get all registered commands.
        /// </summary>
        public IEnumerable<ICommand> GetAllCommands() => _commands.Values;

        /// <summary>
        /// Clear all registered commands.
        /// </summary>
        public void Clear() {
            _commands.Clear();
            _aliases.Clear();
        }
    }
}