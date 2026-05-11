// Copyright 2026 Spellbound Studio Inc.

namespace Spellbound.Core {
    public interface IChunkMigrationCoordinator {
        void RequestMigration(IMigratable migratable);
    }
}