// Copyright 2026 Spellbound Studio Inc.

namespace Spellbound.Core.Logging {
    public interface ILogSink {
        string DisplayName { get; }
        void Emit(LogLevel level, string source, string message);
    }
}