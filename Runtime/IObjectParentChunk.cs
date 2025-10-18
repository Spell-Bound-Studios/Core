// Copyright 2025 Spellbound Studio Inc.

using System;
using UnityEngine;

namespace Spellbound.Core {
    /// <summary>
    /// Contract for registering and unregistering network governed objects.
    /// </summary>
    public interface IObjectParentChunk {
        public event Action<Bounds> ReParentingCheck;
        public event Action ChunkReady;
        public Transform GetRegistryTransform();

        public void SwapInPersistent(
            string presetUid, int generationIndex, Vector3 position, Quaternion rotation, float scale,
            SbbData[] sbbDatas);

        public void SpawnPersistent(
            string presetUid, Vector3 position, Quaternion rotation, float scale, SbbData[] sbbDatas);
    }
}