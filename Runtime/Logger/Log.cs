// Copyright 2026 Spellbound Studio Inc.

using System.Diagnostics;

namespace Spellbound.Core.Logging {
    public static class Log {
        private static ILogSink[] _sinks = System.Array.Empty<ILogSink>();

        public static void AddSink(ILogSink sink) {
            var old = _sinks;
            var next = new ILogSink[old.Length + 1];
            System.Array.Copy(old, next, old.Length);
            next[old.Length] = sink;
            _sinks = next;
        }

        [Conditional("SPELLBOUND_LOG_VERBOSE")]
        public static void Verbose(string source, string message) => Emit(LogLevel.Verbose, source, message);

        [Conditional("SPELLBOUND_LOG_DEBUG")]
        public static void Debug(string source, string message) => Emit(LogLevel.Debug, source, message);

        [Conditional("SPELLBOUND_LOG_INFO")]
        public static void Info(string source, string message) => Emit(LogLevel.Info, source, message);

        [Conditional("SPELLBOUND_LOG_WARNING")]
        public static void Warning(string source, string message) => Emit(LogLevel.Warning, source, message);

        public static void Error(string source, string message) => Emit(LogLevel.Error, source, message);

        private static void Emit(LogLevel level, string source, string message) {
            var sinks = _sinks;
            foreach (var sink in sinks)
                sink.Emit(level, source, message);
        }
    }
}