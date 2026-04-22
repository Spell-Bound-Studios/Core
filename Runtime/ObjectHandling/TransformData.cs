// Copyright 2026 Spellbound Studio Inc.

using System;
using Spellbound.Core.Packing;
using UnityEngine;

namespace Spellbound.Core {
    public struct TransformData : IPacker {
        public Vector3 Position;
        public Vector3 Rotation;
        public float Scale;

        public TransformData(Vector3 position, Vector3 rotation, float scale) {
            Position = position;
            Rotation = rotation;
            Scale = scale;
        }

        public void Pack(ref Span<byte> buffer) {
            Packer.WriteVector3(ref buffer, Position);
            Packer.WriteVector3(ref buffer, Rotation);
            Packer.WriteFloat(ref buffer, Scale);
        }

        public void Unpack(ref ReadOnlySpan<byte> buffer) {
            Position = Packer.ReadVector3(ref buffer);
            Rotation = Packer.ReadVector3(ref buffer);
            Scale = Packer.ReadFloat(ref buffer);
        }
    }
}