// Copyright 2026 Spellbound Studio Inc.

using UnityEngine;

namespace Spellbound.Core {
    public readonly struct MigrationRequest {
        public readonly int InstanceIndex;
        public readonly Vector3Int SourceChunk;
        public readonly Vector3Int DestinationChunk;
        public readonly bool IsSeedDerived;

        public MigrationRequest(int instanceIndex, Vector3Int sourceChunk, Vector3Int destinationChunk, bool isSeedDerived) {
            InstanceIndex = instanceIndex;
            SourceChunk = sourceChunk;
            DestinationChunk = destinationChunk;
            IsSeedDerived = isSeedDerived;
        }
    }
}