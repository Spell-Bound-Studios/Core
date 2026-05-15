// Copyright 2026 Spellbound Studio Inc.

using Spellbound.Core.ObjectData;
using Spellbound.Core.Objects;

namespace Spellbound.Core.PresetContracts {
    public interface IMouseoverHandler {
        string GetTooltip(ObjectPreset preset);
    }

    public interface IMouseoverHandler<T> : IMouseoverHandler where T : IDecodableData {
        string GetTooltip(T data, ObjectPreset preset);
    }
}