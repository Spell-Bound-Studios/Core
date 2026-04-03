// Copyright 2025 Spellbound Studio Inc.

using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Spellbound.Core {
    /// <summary>
    /// This struct represents a procedurally generated GoData.
    /// Its custom data must be inserted separately outside of jobs
    /// </summary>
    [Serializable]
    public struct PristineGoData {
        public float3 position;
        public float3 rotation;
        public float3 scale;
        public Entity entityPrefab;
        public PristineGoData(
            float3 pos,
            float3 rot,
            float3 sc,
            Entity prefab
        ) {
            position = pos;
            rotation = rot;
            scale = sc;
            entityPrefab = prefab;
        }

        public override string ToString() =>
                $"GoData at: {position}, {rotation}, {scale}, {entityPrefab}";
    }
}