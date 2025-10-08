using System;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

namespace SpellBound.Core {
    public static class FastPacker {
        #region Bool

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteBool(ref Span<byte> buffer, bool value) {
            buffer[0] = value ? (byte)1 : (byte)0;
            buffer = buffer[1..];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ReadBool(ref ReadOnlySpan<byte> buffer) {
            var value = buffer[0] != 0;
            buffer = buffer[1..];
            return value;
        }

        #endregion
        
        #region Int

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteInt(ref Span<byte> buffer, int value) {
            BitConverter.TryWriteBytes(buffer, value);
            buffer = buffer[sizeof(int)..];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ReadInt(ref ReadOnlySpan<byte> buffer) {
            var value = BitConverter.ToInt32(buffer);
            buffer = buffer[sizeof(int)..];

            return value;
        }

        #endregion

        #region Floats

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteFloat(ref Span<byte> buffer, float value) {
            BitConverter.TryWriteBytes(buffer, value);
            buffer = buffer[sizeof(float)..];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ReadFloat(ref ReadOnlySpan<byte> buffer) {
            var value = BitConverter.ToSingle(buffer);
            buffer = buffer[sizeof(float)..];

            return value;
        }

        #endregion

        #region Strings

        public static void WriteString(ref Span<byte> buffer, string value) {
            value ??= string.Empty;
            var bytes = Encoding.UTF8.GetBytes(value);
            WriteInt(ref buffer, bytes.Length);
            bytes.AsSpan().CopyTo(buffer);
            buffer = buffer[bytes.Length..];
        }

        public static string ReadString(ref ReadOnlySpan<byte> buffer) {
            var len = ReadInt(ref buffer);
            var value = Encoding.UTF8.GetString(buffer[..len]);
            buffer = buffer[len..];

            return value;
        }

        #endregion

        #region Vector3

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteVector3(ref Span<byte> buffer, in Vector3 v) {
            WriteFloat(ref buffer, v.x);
            WriteFloat(ref buffer, v.y);
            WriteFloat(ref buffer, v.z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ReadVector3(ref ReadOnlySpan<byte> buffer) {
            var x = ReadFloat(ref buffer);
            var y = ReadFloat(ref buffer);
            var z = ReadFloat(ref buffer);

            return new Vector3(x, y, z);
        }

        #endregion

        #region Vector2

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteVector2(ref Span<byte> buffer, in Vector2 v) {
            WriteFloat(ref buffer, v.x);
            WriteFloat(ref buffer, v.y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 ReadVector2(ref ReadOnlySpan<byte> buffer) {
            var x = ReadFloat(ref buffer);
            var y = ReadFloat(ref buffer);

            return new Vector2(x, y);
        }

        #endregion

        #region Quaternion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteQuaternion(ref Span<byte> buffer, in Quaternion q) {
            WriteFloat(ref buffer, q.x);
            WriteFloat(ref buffer, q.y);
            WriteFloat(ref buffer, q.z);
            WriteFloat(ref buffer, q.w);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Quaternion ReadQuaternion(ref ReadOnlySpan<byte> buffer) {
            var x = ReadFloat(ref buffer);
            var y = ReadFloat(ref buffer);
            var z = ReadFloat(ref buffer);
            var w = ReadFloat(ref buffer);

            return new Quaternion(x, y, z, w);
        }

        #endregion
    }
}