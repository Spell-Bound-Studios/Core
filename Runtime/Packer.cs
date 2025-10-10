// Copyright 2025 Spellbound Studio Inc.

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

namespace SpellBound.Core {
    /// <summary>
    /// Static binary serialization utility using Span-based APIs.
    /// Provides both BitConverter and bitwise methods for flexibility.
    /// Requires IPacker implementers.
    /// </summary>
    public static class Packer {
        // Starting stack buffer size
        private const int StackBufferSize = 4096;

        // Max heap buffer allowed: 64 MB - tune as needed
        private const int MaxRentedBuffer = 1024 * 1024 * 64;

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteIntBitwise(ref Span<byte> buffer, int value) {
            buffer[0] = (byte)value;
            buffer[1] = (byte)(value >> 8);
            buffer[2] = (byte)(value >> 16);
            buffer[3] = (byte)(value >> 24);
            buffer = buffer[4..];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ReadIntBitwise(ref ReadOnlySpan<byte> buffer) {
            var value = buffer[0] | (buffer[1] << 8) | (buffer[2] << 16) | (buffer[3] << 24);
            buffer = buffer[4..];

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteFloatBitwise(ref Span<byte> buffer, float value) {
            var intVal = BitConverter.SingleToInt32Bits(value);
            WriteIntBitwise(ref buffer, intVal);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ReadFloatBitwise(ref ReadOnlySpan<byte> buffer) {
            var intVal = ReadIntBitwise(ref buffer);

            return BitConverter.Int32BitsToSingle(intVal);
        }

        #endregion

        #region ULong

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteULong(ref Span<byte> buffer, ulong value) {
            BitConverter.TryWriteBytes(buffer, value);
            buffer = buffer[sizeof(ulong)..];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong ReadULong(ref ReadOnlySpan<byte> buffer) {
            var value = BitConverter.ToUInt64(buffer);
            buffer = buffer[sizeof(ulong)..];

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteULongBitwise(ref Span<byte> buffer, ulong value) {
            buffer[0] = (byte)value;
            buffer[1] = (byte)(value >> 8);
            buffer[2] = (byte)(value >> 16);
            buffer[3] = (byte)(value >> 24);
            buffer[4] = (byte)(value >> 32);
            buffer[5] = (byte)(value >> 40);
            buffer[6] = (byte)(value >> 48);
            buffer[7] = (byte)(value >> 56);
            buffer = buffer[8..];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong ReadULongBitwise(ref ReadOnlySpan<byte> buffer) {
            var value = (ulong)buffer[0]
                        | ((ulong)buffer[1] << 8)
                        | ((ulong)buffer[2] << 16)
                        | ((ulong)buffer[3] << 24)
                        | ((ulong)buffer[4] << 32)
                        | ((ulong)buffer[5] << 40)
                        | ((ulong)buffer[6] << 48)
                        | ((ulong)buffer[7] << 56);
            buffer = buffer[8..];

            return value;
        }

        #endregion

        #region Long

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteLong(ref Span<byte> buffer, long value) {
            BitConverter.TryWriteBytes(buffer, value);
            buffer = buffer[sizeof(long)..];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long ReadLong(ref ReadOnlySpan<byte> buffer) {
            var value = BitConverter.ToInt64(buffer);
            buffer = buffer[sizeof(long)..];

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteLongBitwise(ref Span<byte> buffer, long value) {
            buffer[0] = (byte)value;
            buffer[1] = (byte)(value >> 8);
            buffer[2] = (byte)(value >> 16);
            buffer[3] = (byte)(value >> 24);
            buffer[4] = (byte)(value >> 32);
            buffer[5] = (byte)(value >> 40);
            buffer[6] = (byte)(value >> 48);
            buffer[7] = (byte)(value >> 56);
            buffer = buffer[8..];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long ReadLongBitwise(ref ReadOnlySpan<byte> buffer) {
            var value = (long)buffer[0]
                        | ((long)buffer[1] << 8)
                        | ((long)buffer[2] << 16)
                        | ((long)buffer[3] << 24)
                        | ((long)buffer[4] << 32)
                        | ((long)buffer[5] << 40)
                        | ((long)buffer[6] << 48)
                        | ((long)buffer[7] << 56);
            buffer = buffer[8..];

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteVector3Bitwise(ref Span<byte> buffer, in Vector3 v) {
            WriteFloatBitwise(ref buffer, v.x);
            WriteFloatBitwise(ref buffer, v.y);
            WriteFloatBitwise(ref buffer, v.z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ReadVector3Bitwise(ref ReadOnlySpan<byte> buffer) {
            var x = ReadFloatBitwise(ref buffer);
            var y = ReadFloatBitwise(ref buffer);
            var z = ReadFloatBitwise(ref buffer);

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteVector2Bitwise(ref Span<byte> buffer, in Vector2 v) {
            WriteFloatBitwise(ref buffer, v.x);
            WriteFloatBitwise(ref buffer, v.y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 ReadVector2Bitwise(ref ReadOnlySpan<byte> buffer) {
            var x = ReadFloatBitwise(ref buffer);
            var y = ReadFloatBitwise(ref buffer);

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteQuaternionBitwise(ref Span<byte> buffer, in Quaternion q) {
            WriteFloatBitwise(ref buffer, q.x);
            WriteFloatBitwise(ref buffer, q.y);
            WriteFloatBitwise(ref buffer, q.z);
            WriteFloatBitwise(ref buffer, q.w);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Quaternion ReadQuaternionBitwise(ref ReadOnlySpan<byte> buffer) {
            var x = ReadFloatBitwise(ref buffer);
            var y = ReadFloatBitwise(ref buffer);
            var z = ReadFloatBitwise(ref buffer);
            var w = ReadFloatBitwise(ref buffer);

            return new Quaternion(x, y, z, w);
        }

        #endregion

        #region Byte Arrays

        /// <summary>
        /// Write a byte array with a length prefix (null treated as empty)
        /// </summary>
        public static void WriteBytes(ref Span<byte> buffer, byte[] data) {
            var len = data?.Length ?? 0;
            WriteInt(ref buffer, len);

            if (len <= 0) 
                return;

            data.AsSpan().CopyTo(buffer);
            buffer = buffer[len..];
        }

        /// <summary>
        /// Write a ReadOnlySpan with a length prefix
        /// </summary>
        public static void WriteBytes(ref Span<byte> buffer, ReadOnlySpan<byte> data) {
            WriteInt(ref buffer, data.Length);
            data.CopyTo(buffer);
            buffer = buffer[data.Length..];
        }

        /// <summary>
        /// Read a length-prefixed byte array (never returns null, returns empty array)
        /// </summary>
        public static byte[] ReadBytes(ref ReadOnlySpan<byte> buffer) {
            var len = ReadInt(ref buffer);
            if (len == 0) 
                return Array.Empty<byte>();
            
            var result = buffer[..len].ToArray();
            buffer = buffer[len..];
            return result;
        }

        #endregion

        #region Array Packing

        /// <summary>
        /// Pack an array of IPacker items with length prefix (null treated as empty)
        /// </summary>
        public static void PackArray<T>(ref Span<byte> buffer, T[] items) where T : IPacker {
            var count = items?.Length ?? 0;
            WriteInt(ref buffer, count);
            
            if (items == null) return;
            
            foreach (var item in items) {
                item.Pack(ref buffer);
            }
        }

        /// <summary>
        /// Unpack an array of IPacker items (never returns null, returns empty array)
        /// </summary>
        public static T[] UnpackArray<T>(ref ReadOnlySpan<byte> buffer) where T : IPacker, new() {
            var count = ReadInt(ref buffer);
            if (count == 0) return Array.Empty<T>();

            var result = new T[count];
            for (var i = 0; i < count; i++) {
                var item = new T();
                item.Unpack(ref buffer);
                result[i] = item;
            }
            return result;
        }

        #endregion

        #region List Packing

        /// <summary>
        /// Pack a List of IPacker items with length prefix (null treated as empty)
        /// </summary>
        public static void PackList<T>(ref Span<byte> buffer, List<T> items) where T : IPacker {
            var count = items?.Count ?? 0;
            WriteInt(ref buffer, count);
            
            if (items == null) return;

            foreach (var item in items) {
                item.Pack(ref buffer);
            }
        }

        /// <summary>
        /// Unpack a List of IPacker items (never returns null, returns empty list)
        /// </summary>
        public static List<T> UnpackList<T>(ref ReadOnlySpan<byte> buffer) where T : IPacker, new() {
            var count = ReadInt(ref buffer);
            if (count == 0) return new List<T>();

            var result = new List<T>(count);
            for (var i = 0; i < count; i++) {
                var item = new T();
                item.Unpack(ref buffer);
                result.Add(item);
            }
            return result;
        }

        /// <summary>
        /// Pack a List of int with length prefix (null treated as empty)
        /// </summary>
        public static void PackIntList(ref Span<byte> buffer, List<int> items) {
            var count = items?.Count ?? 0;
            WriteInt(ref buffer, count);
            
            if (items == null) return;

            foreach (var item in items) {
                WriteInt(ref buffer, item);
            }
        }

        /// <summary>
        /// Unpack a List of int (never returns null, returns empty list)
        /// </summary>
        public static List<int> UnpackIntList(ref ReadOnlySpan<byte> buffer) {
            var count = ReadInt(ref buffer);
            if (count == 0) return new List<int>();

            var result = new List<int>(count);
            for (var i = 0; i < count; i++) {
                result.Add(ReadInt(ref buffer));
            }
            return result;
        }

        #endregion

        #region High-Level Helpers

        /// <summary>
        /// Serialize a value-type IPacker into a byte[].
        /// Uses stack buffer for small payloads, ArrayPool for larger ones.
        /// </summary>
        public static byte[] ToBytes<T>(in T obj) where T : IPacker {
            // Try stack buffer first for small payloads
            Span<byte> stackBuf = stackalloc byte[StackBufferSize];
            var span = stackBuf;

            try {
                obj.Pack(ref span);
                var written = stackBuf.Length - span.Length;
                return stackBuf[..written].ToArray();
            }
            catch (ArgumentOutOfRangeException) { }

            // Payload too large, use ArrayPool with exponential growth
            var size = Math.Max(StackBufferSize * 2, 8192);

            while (size <= MaxRentedBuffer) {
                var rented = ArrayPool<byte>.Shared.Rent(size);
                try {
                    var rentedSpan = new Span<byte>(rented, 0, size);
                    var working = rentedSpan;

                    try {
                        obj.Pack(ref working);
                        var written = size - working.Length;

                        var result = new byte[written];
                        Buffer.BlockCopy(rented, 0, result, 0, written);
                        return result;
                    }
                    catch (ArgumentOutOfRangeException) {
                        // Need bigger buffer
                    }
                }
                finally {
                    ArrayPool<byte>.Shared.Return(rented);
                }

                size = Math.Min(size * 2, MaxRentedBuffer);
            }

            throw new InvalidOperationException($"Payload exceeds maximum buffer size of {MaxRentedBuffer} bytes");
        }

        /// <summary>
        /// Deserialize a struct that implements IPacker from a byte array or ReadOnlySpan
        /// </summary>
        public static T FromBytes<T>(ReadOnlySpan<byte> bytes) where T : IPacker, new() {
            var span = bytes;
            var obj = new T();
            obj.Unpack(ref span);
            return obj;
        }

        /// <summary>
        /// Pack an array directly to bytes (convenience method, null treated as empty)
        /// </summary>
        public static byte[] PackArrayToBytes<T>(T[] items) where T : IPacker {
            var count = items?.Length ?? 0;
            
            // Handle empty/null case
            if (count == 0) {
                Span<byte> emptyBuf = stackalloc byte[4];
                var emptySpan = emptyBuf;
                WriteInt(ref emptySpan, 0);
                return emptyBuf.ToArray();
            }

            Span<byte> stackBuf = stackalloc byte[StackBufferSize];
            var span = stackBuf;

            try {
                PackArray(ref span, items);
                var written = stackBuf.Length - span.Length;
                return stackBuf[..written].ToArray();
            }
            catch (ArgumentOutOfRangeException) { }

            // Need larger buffer
            var size = Math.Max(StackBufferSize * 2, 8192);

            while (size <= MaxRentedBuffer) {
                var rented = ArrayPool<byte>.Shared.Rent(size);
                try {
                    var rentedSpan = new Span<byte>(rented, 0, size);
                    var working = rentedSpan;

                    try {
                        PackArray(ref working, items);
                        var written = size - working.Length;

                        var result = new byte[written];
                        Buffer.BlockCopy(rented, 0, result, 0, written);
                        return result;
                    }
                    catch (ArgumentOutOfRangeException) { }
                }
                finally {
                    ArrayPool<byte>.Shared.Return(rented);
                }

                size = Math.Min(size * 2, MaxRentedBuffer);
            }

            throw new InvalidOperationException($"Array payload exceeds maximum buffer size of {MaxRentedBuffer} bytes");
        }

        /// <summary>
        /// Unpack an array directly from bytes (convenience method, never returns null)
        /// </summary>
        public static T[] UnpackArrayFromBytes<T>(byte[] bytes) where T : IPacker, new() {
            if (bytes == null || bytes.Length == 0) return Array.Empty<T>();
            ReadOnlySpan<byte> span = bytes;
            return UnpackArray<T>(ref span);
        }

        /// <summary>
        /// Equality check for packed item data.
        /// </summary>
        public static bool AreBytesEqual(byte[] a, byte[] b) {
            if (a == null && b == null) return true;

            // If the byte array is null or empty return true.
            // TODO: This potentially has implications on craftable material items but beyond scope for now.
            if (NoData(a) && NoData(b)) return true;

            if (a == null || b == null) return false;
            if (a.Length != b.Length) return false;

            for (var i = 0; i < a.Length; ++i) {
                if (a[i] != b[i])
                    return false;
            }

            return true;

            // Helper method that returns a bool if the byte array is null or empty.
            static bool NoData(byte[] x) => x == null || x.Length == 0;
        }

        #endregion
    }
}