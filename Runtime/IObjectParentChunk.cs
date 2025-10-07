using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace SpellBound.Core {
    /// <summary>
    /// Contract for registering and unregistering network governed objects.
    /// </summary>
    public interface IObjectParentChunk {

        public event Action<Bounds> ReParentingCheck;
        public Transform GetRegistryTransform();
        
        public void SwapInPersistent(string presetUid, int generationIndex, Vector3 position, Quaternion rotation, float scale, SbbData[] sbbDatas);
        
        public void SpawnPersistent(string presetUid, Vector3 position, Quaternion rotation, float scale, SbbData[] sbbDatas);
    }
}