// Copyright 2026 Spellbound Studio Inc.

namespace Spellbound.Core {
    public class MainTooltipModule : PresetModule, IDispatch<MouseoverContext>, IMouseoverHandler {
        public bool OnDispatch(
            MouseoverContext dispatchContext, IObjectParent parent, int instanceIndex, ObjectPreset preset,
            int eventSurfaceIndex) {
            TooltipEvents.Invoke(GetTooltip(preset));

            return true;
        }

        public string GetTooltip(ObjectPreset preset) => preset.objectName;
    }
}