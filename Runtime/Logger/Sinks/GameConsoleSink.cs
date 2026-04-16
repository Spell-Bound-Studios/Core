// Copyright 2026 Spellbound Studio Inc.

namespace Spellbound.Core.Logging {
    public class GameConsoleSink : ILogSink {
        private const string DisplayNameValue = "Game Console";

        public string DisplayName => DisplayNameValue;

        public void Initialize(LogConfig config) { }

        public void Emit(LogLevel level, string source, string message) {
            var formatted = $"[{source}] {message}";

            if (level >= LogLevel.Error)
                Console.ConsoleLogger.PrintError(formatted);
            else
                Console.ConsoleLogger.PrintToConsole(formatted);
        }
    }
}