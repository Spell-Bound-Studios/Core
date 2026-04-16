// Copyright 2026 Spellbound Studio Inc.

using System.Diagnostics;

namespace Spellbound.Core.Logging {
    public static class Log {
        private static RegisteredSink[] _sinks = System.Array.Empty<RegisteredSink>();
        
        public static void AddSink(ILogSink sink, LogConfig config, LogLevel filterLevel) {
            sink.Initialize(config);

            var old = _sinks;
            var next = new RegisteredSink[old.Length + 1];
            System.Array.Copy(old, next, old.Length);
            next[old.Length] = new RegisteredSink { Sink = sink, FilterLevel = filterLevel };
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
            for (var i = 0; i < sinks.Length; i++) {
                if (level < sinks[i].FilterLevel)
                    continue;
                sinks[i].Sink.Emit(level, source, message);
            }
        }
        
        private struct RegisteredSink {
            public ILogSink Sink;
            public LogLevel FilterLevel;
        }
    }
}