// Copyright 2025 Spellbound Studio Inc.

namespace Spellbound.Core.Console {
    /// <summary>
    /// Static logging utility that prints directly to the console in build or in the editor.
    /// Useful for debugging and adding capability to your packages.
    /// </summary>
    public static class ConsoleLogger {
        private static ConsoleController _consoleControllerInstance;
        
        /// <summary>
        /// Initialize the logger with a console instance.
        /// Called automatically by ConsoleController on Awake.
        /// </summary>
        internal static void Initialize(ConsoleController console) {
            _consoleControllerInstance = console;
        }

        /// <summary>
        /// Print a message to the developer console.
        /// </summary>
        public static void PrintToConsole(string message) {
            if (_consoleControllerInstance != null)
                _consoleControllerInstance.LogOutput(message);
        }

        /// <summary>
        /// Print an error message to the developer console.
        /// </summary>
        public static void PrintError(string message) {
            if (_consoleControllerInstance != null)
                _consoleControllerInstance.LogError(message);
        }

        /// <summary>
        /// Check if the console logger is initialized and ready to use.
        /// </summary>
        public static bool IsInitialized => _consoleControllerInstance != null;
    }
}