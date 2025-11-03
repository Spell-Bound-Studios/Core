// Copyright 2025 Spellbound Studio Inc.

using Spellbound.Core.Console;
using UnityEngine;

namespace Spellbound.Core {
    public static class SbCore {
        /// <summary>
        /// Public method for easily spawning a persistent object preset in the world.
        /// </summary>
        [ConsoleCommandMethod("spawn", typeof(GameObjectModule))]
        public static void SpawnObjectPreset(
            string presetUid, Vector3 spawnPosition, float scale,  SbbData spawnData) {
            
            if (!SingletonManager.TryGetSingletonInstance<IChunkManager>(out var icm)) {
                Debug.LogError("The singleton manager does not contain an IChunkManager. Please ensure you implement" +
                               "IChunkManager or drag the prefab into your scene.");
                return;
            }
            
            var iChunk = icm.GetObjectParentChunk(spawnPosition);

            iChunk.SpawnPersistent(
                presetUid, 
                spawnPosition, 
                Quaternion.identity, 
                scale, 
                new[] {
                    spawnData
                });
        }
    }
}