// Copyright 2026 Spellbound Studio Inc.

using System;

namespace Spellbound.Core {
    public class MainTooltipModule : PresetModule, IDispatch<MouseoverContext>, IMouseoverHandler {
        
        public void OnDispatch(
            MouseoverContext dispatchContext, IObjectParent parent, int instanceIndex, ObjectPreset preset,
            int eventSurfaceIndex) {

            if (preset.TryGetModule(out IMouseoverHandler handler, eventSurfaceIndex)) {
                TooltipEvents.Invoke(handler.GetTooltip(preset));
            }
        }

        public string GetTooltip(ObjectPreset preset) => preset.objectName;
    }
}