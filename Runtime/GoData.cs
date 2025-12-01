// Copyright 2025 Spellbound Studio Inc.

using System;
using Spellbound.Core.Packing;
using UnityEngine;

namespace Spellbound.Core {
    /// <summary>
    /// This struct represents ANY (networked and not networked) game object in a chunk.
    /// </summary>
    [Serializable]
    public struct GoData : IPacker {
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

        public static GoData Empty =>
                new() {
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

        public void Pack(ref Span<byte> buffer) {
            Packer.WriteVector3(ref buffer, position);
            Packer.WriteVector3(ref buffer, rotation);
            Packer.WriteVector3(ref buffer, scale);
            Packer.WriteString(ref buffer, presetUid);
            Packer.PackArray(ref buffer, SbbDatas);
        }

        public void Unpack(ref ReadOnlySpan<byte> buffer) {
            position = Packer.ReadVector3(ref buffer);
            rotation = Packer.ReadVector3(ref buffer);
            scale = Packer.ReadVector3(ref buffer);
            presetUid = Packer.ReadString(ref buffer);
            SbbDatas = Packer.UnpackArray<SbbData>(ref buffer);
        }
    }
}