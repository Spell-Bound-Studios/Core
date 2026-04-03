// Copyright 2025 Spellbound Studio Inc.

using Spellbound.Core.Console;
using UnityEngine;

namespace Spellbound.Core {
    public static class SbCore {
        /// <summary>
        /// Public method for easily spawning a persistent object preset in the world.
        /// </summary>
        [ConsolePresetCommand("spawn", typeof(ObjectPreset))]
        public static void SpawnObjectPreset(
            ObjectPreset preset, Vector3 spawnPosition) {
            
            if (!SingletonManager.TryGetSingletonInstance<IChunkManager>(out var icm)) {
                Debug.LogError("The singleton manager does not contain an IChunkManager. Please ensure you implement" +
                               "IChunkManager or drag the bakePrefab into your scene.");
                return;
            }
            if (!icm.TryGetObjectParentChunk(spawnPosition, out var chunk))
                return;

            chunk.ObjectParent.CreateNewInstance(preset, spawnPosition, Vector3.zero, 1);
        }
    }
}