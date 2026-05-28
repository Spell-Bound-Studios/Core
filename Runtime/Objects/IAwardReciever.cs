// Copyright 2026 Spellbound Studio Inc.

using Spellbound.Core.Packing;

namespace Spellbound.Core.Objects {
    public interface IAwardReciever {
        void Award<T>(T award) where T : ISmartPacker;
    }
}