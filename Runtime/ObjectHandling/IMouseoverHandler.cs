// Copyright 2026 Spellbound Studio Inc.

namespace Spellbound.Core {
    public interface IMouseoverHandler {
        string GetTooltip(ObjectPreset preset);
    }

    public interface IMouseoverHandler<T> : IMouseoverHandler where T : IDecodableData {
        string GetTooltip(T data, ObjectPreset preset);
    }
}