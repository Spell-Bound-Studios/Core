// Copyright 2026 Spellbound Studio Inc.

namespace Spellbound.Core {
    public interface IObjectParent {
        public ObjectParent ObjectParent { get; }
        
        // wraps ObjectParent's public methods as default implementations
    }
}