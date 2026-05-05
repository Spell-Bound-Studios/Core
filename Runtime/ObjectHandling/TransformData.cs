// Copyright 2026 Spellbound Studio Inc.

using System;
using Spellbound.Core.Packing;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Spellbound.Core {
    public struct TransformData : IPacker {
        public Vector3 Position;
        public Vector3 Rotation;
        public float Scale;
        
        #region Constructors

        
        // Default Constructor
        public TransformData(Vector3 position, Vector3 rotation, float scale) {
            Position = position;
            Rotation = rotation;
            Scale = scale;
        }

        // Constructor from ECS transform
        public TransformData(LocalTransform localTransform) {
            Position = localTransform.Position;
            Rotation = math.degrees(math.EulerXYZ(localTransform.Rotation));
            Scale = localTransform.Scale;
        }

        // Constructor from GameObject Transform
        public TransformData(Transform transform) {
            Position = transform.position;
            Rotation = transform.rotation.eulerAngles;
            Scale = transform.localScale.x;
        }
        
        #endregion

        #region HelperReadMethdods
        
        public readonly LocalTransform ToLocalTransform() {
            return new LocalTransform() {
                Position = Position,
                Rotation = quaternion.EulerXYZ(Rotation),
                Scale = Scale
            };
        }

        public Quaternion RotAsQuaternion() {
            return quaternion.EulerXYZ(Rotation);
        }
        
        public Vector3 ScaleAsVector3() {
            return new  Vector3(Scale, Scale, Scale);
        }
        
        #endregion
        
        #region Packing
        
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
        
        #endregion

        
    }
}