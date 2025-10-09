using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

namespace SpellBound.Core {
    /// <summary>
    /// Static binary serialization utility using Span-based APIs.
    /// Provides both BitConverter and bitwise methods for flexibility.
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
        /// Write a byte array with length prefix
        /// </summary>
        public static void WriteBytes(ref Span<byte> buffer, ReadOnlySpan<byte> data) {
            WriteInt(ref buffer, data.Length);
            data.CopyTo(buffer);
            buffer = buffer[data.Length..];
        }

        /// <summary>
        /// Read a length-prefixed byte array
        /// </summary>
        public static byte[] ReadBytes(ref ReadOnlySpan<byte> buffer) {
            var len = ReadInt(ref buffer);
            var result = buffer[..len].ToArray();
            buffer = buffer[len..];

            return result;
        }

        #endregion

        #region Array Packing

        /// <summary>
        /// Pack an array of IPacker items with length prefix
        /// </summary>
        public static void PackArray<T>(ref Span<byte> buffer, T[] items) where T : IPacker {
            WriteInt(ref buffer, items.Length);
            foreach (var item in items) item.Pack(ref buffer);
        }

        /// <summary>
        /// Unpack an array of IPacker items
        /// </summary>
        public static T[] UnpackArray<T>(ref ReadOnlySpan<byte> buffer) where T : IPacker, new() {
            var count = ReadInt(ref buffer);
            var result = new T[count];

            for (var i = 0; i < count; i++) {
                var item = new T();
                item.Unpack(ref buffer);
                result[i] = item;
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

            while (true) {
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
                        Debug.Log("Bigger buffer required from ToBytes in Packer.");
                    }
                }
                finally {
                    ArrayPool<byte>.Shared.Return(rented);
                }

                size = Math.Min(size * 2, MaxRentedBuffer);
            }
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
        /// Pack an array directly to bytes (convenience method)
        /// </summary>
        public static byte[] PackArrayToBytes<T>(T[] items) where T : IPacker {
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

            while (true) {
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
        }

        /// <summary>
        /// Unpack an array directly from bytes (convenience method)
        /// </summary>
        public static T[] UnpackArrayFromBytes<T>(byte[] bytes) where T : IPacker, new() {
            ReadOnlySpan<byte> span = bytes;

            return UnpackArray<T>(ref span);
        }

        #endregion
    }
}