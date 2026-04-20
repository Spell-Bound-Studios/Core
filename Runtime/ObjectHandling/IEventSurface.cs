// Copyright 2026 Spellbound Studio Inc.

using System;
using Spellbound.Core.Logging;

namespace Spellbound.Core {
    public interface IEventSurface {
        void Initialize(IObjectParent iobjectParent, int entityIndex, string presetUid);

        public void DebugQueryPing();

        public void Dispatch<THandler>(Action<THandler, IObjectParent, int, string, int> invoke)
                where THandler : class;
    }
}