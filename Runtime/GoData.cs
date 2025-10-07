using System;
using System.Collections.Generic;
using PurrNet.Packing;
using Unity.Collections;
using UnityEngine;

namespace SpellBound.Core {
    /// <summary>
    /// This struct represents ANY (networked and not networked) gameobject in a chunk.
    /// </summary>
    [Serializable]
    public struct GoData : IPackedAuto {
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;
        public string presetUid;
        public SbbData[] SbbDatas;
        
        public GoData(
            Vector3 pos,
            Vector3 rot,
            Vector3 sc,
            string id,
            SbbData[] sbbDatas = null
            ) {
            // Transform information
            position = pos;
            rotation = rot;
            scale = sc;
            
            // Object type and the custom data that lives on it.
            presetUid = id;
            SbbDatas = sbbDatas;
        }
        
        public static GoData Empty => new GoData {
            position = Vector3.zero,
            rotation = Vector3.zero,
            scale = Vector3.one,
            presetUid = string.Empty,
            SbbDatas = null
        };

        public override string ToString() {
            var data = $"GoData at: {position}, {rotation}, {scale}, {presetUid}";
            return data;
        }
    }
}