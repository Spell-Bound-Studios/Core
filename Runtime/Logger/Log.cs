// Copyright 2026 Spellbound Studio Inc.

using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

namespace Spellbound.Core.Logging {
    public static class Log {
        private struct RegisteredSink {
            public ILogSink Sink;
            public LogLevel FilterLevel;
        }

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
        public static void Verbose(
            string message,
            [CallerFilePath] string file = "",
            [CallerMemberName] string member = "",
            [CallerLineNumber] int line = 0
        ) =>
                Emit(LogLevel.Verbose, message, file, member, line);

        [Conditional("SPELLBOUND_LOG_DEBUG")]
        public static void Debug(
            string message,
            [CallerFilePath] string file = "",
            [CallerMemberName] string member = "",
            [CallerLineNumber] int line = 0
        ) =>
                Emit(LogLevel.Debug, message, file, member, line);

        [Conditional("SPELLBOUND_LOG_INFO")]
        public static void Info(
            string message,
            [CallerFilePath] string file = "",
            [CallerMemberName] string member = "",
            [CallerLineNumber] int line = 0
        ) =>
                Emit(LogLevel.Info, message, file, member, line);

        [Conditional("SPELLBOUND_LOG_WARNING")]
        public static void Warn(
            string message,
            [CallerFilePath] string file = "",
            [CallerMemberName] string member = "",
            [CallerLineNumber] int line = 0
        ) =>
                Emit(LogLevel.Warning, message, file, member, line);

        public static void Error(
            string message,
            [CallerFilePath] string file = "",
            [CallerMemberName] string member = "",
            [CallerLineNumber] int line = 0
        ) =>
                Emit(LogLevel.Error, message, file, member, line);

        public static void Emit(LogLevel level, string message, string file, string member, int line) {
            var source = Path.GetFileNameWithoutExtension(file);
            var sinks = _sinks;

            for (var i = 0; i < sinks.Length; i++) {
                if (level < sinks[i].FilterLevel)
                    continue;

                sinks[i].Sink.Emit(level, source, message, member, line);
            }
        }
    }
}