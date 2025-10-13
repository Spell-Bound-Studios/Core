// Copyright 2025 Spellbound Studio Inc.

namespace SpellBound.Core {
    /// <summary>
    /// Responsible for being implemented on anything that can take damage.
    /// </summary>
    public interface IDamageable {
        void TakeDamage(float damage);
    }
}