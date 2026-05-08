// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using Spellbound.Core.Packing;

namespace Spellbound.Core {
    public interface IMouseoverHandler {
        void OnMouseover(IObjectParent parent, int instanceIndex, string presetUid, int eventSurfaceIndex);
    }
}