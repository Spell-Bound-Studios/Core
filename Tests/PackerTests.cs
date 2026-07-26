// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Spellbound.Core.Packing;
using UnityEngine;

namespace Spellbound.Core.Tests {
    public class PackerTests {
        private struct NumericPayload : IPacker {
            public byte ByteValue;
            public bool BoolValue;
            public short ShortValue;
            public ushort UShortValue;
            public int IntValue;
            public uint UIntValue;
            public long LongValue;
            public ulong ULongValue;
            public float FloatValue;

            public void Pack(ref Span<byte> buffer) {
                Packer.WriteByte(ref buffer, ByteValue);
                Packer.WriteBool(ref buffer, BoolValue);
                Packer.WriteShort(ref buffer, ShortValue);
                Packer.WriteUShort(ref buffer, UShortValue);
                Packer.WriteInt(ref buffer, IntValue);
                Packer.WriteUInt(ref buffer, UIntValue);
                Packer.WriteLong(ref buffer, LongValue);
                Packer.WriteULong(ref buffer, ULongValue);
                Packer.WriteFloat(ref buffer, FloatValue);
            }

            public void Unpack(ref ReadOnlySpan<byte> buffer) {
                ByteValue = Packer.ReadByte(ref buffer);
                BoolValue = Packer.ReadBool(ref buffer);
                ShortValue = Packer.ReadShort(ref buffer);
                UShortValue = Packer.ReadUShort(ref buffer);
                IntValue = Packer.ReadInt(ref buffer);
                UIntValue = Packer.ReadUInt(ref buffer);
                LongValue = Packer.ReadLong(ref buffer);
                ULongValue = Packer.ReadULong(ref buffer);
                FloatValue = Packer.ReadFloat(ref buffer);
            }
        }

        private struct TextPayload : IPacker {
            public string Text;

            public void Pack(ref Span<byte> buffer) => Packer.WriteString(ref buffer, Text);
            public void Unpack(ref ReadOnlySpan<byte> buffer) => Text = Packer.ReadString(ref buffer);
        }

        [Test]
        public void NumericPrimitivesRoundTrip() {
            var payload = new NumericPayload {
                ByteValue = 200,
                BoolValue = true,
                ShortValue = -12345,
                UShortValue = 54321,
                IntValue = int.MinValue,
                UIntValue = uint.MaxValue,
                LongValue = long.MinValue,
                ULongValue = ulong.MaxValue,
                FloatValue = 3.14159f
            };

            var result = Packer.FromBytes<NumericPayload>(Packer.ToBytes(payload));

            Assert.AreEqual(payload, result);
        }

        [Test]
        public void BitwiseVariantsMatchBitConverterLayout() {
            var standard = new byte[32];
            var bitwise = new byte[32];
            var standardSpan = standard.AsSpan();
            var bitwiseSpan = bitwise.AsSpan();

            Packer.WriteShort(ref standardSpan, -1234);
            Packer.WriteUShort(ref standardSpan, 65000);
            Packer.WriteInt(ref standardSpan, -123456789);
            Packer.WriteUInt(ref standardSpan, 3123456789u);
            Packer.WriteLong(ref standardSpan, -1234567890123L);
            Packer.WriteULong(ref standardSpan, 12345678901234567890ul);
            Packer.WriteFloat(ref standardSpan, 1.5f);

            Packer.WriteShortBitwise(ref bitwiseSpan, -1234);
            Packer.WriteUShortBitwise(ref bitwiseSpan, 65000);
            Packer.WriteIntBitwise(ref bitwiseSpan, -123456789);
            Packer.WriteUIntBitwise(ref bitwiseSpan, 3123456789u);
            Packer.WriteLongBitwise(ref bitwiseSpan, -1234567890123L);
            Packer.WriteULongBitwise(ref bitwiseSpan, 12345678901234567890ul);
            Packer.WriteFloatBitwise(ref bitwiseSpan, 1.5f);

            Assert.AreEqual(standard, bitwise);
        }

        [Test]
        public void VectorAndQuaternionRoundTrip() {
            var buffer = new byte[36];
            var writeSpan = buffer.AsSpan();

            Packer.WriteVector3(ref writeSpan, new Vector3(1.5f, -2.5f, 3.5f));
            Packer.WriteVector2(ref writeSpan, new Vector2(-4.5f, 5.5f));
            Packer.WriteQuaternion(ref writeSpan, new Quaternion(0.1f, 0.2f, 0.3f, 0.4f));

            ReadOnlySpan<byte> readSpan = buffer;

            Assert.AreEqual(new Vector3(1.5f, -2.5f, 3.5f), Packer.ReadVector3(ref readSpan));
            Assert.AreEqual(new Vector2(-4.5f, 5.5f), Packer.ReadVector2(ref readSpan));
            Assert.AreEqual(new Quaternion(0.1f, 0.2f, 0.3f, 0.4f), Packer.ReadQuaternion(ref readSpan));
        }

        [Test]
        public void StringsRoundTripIncludingUnicode() {
            var unicode = Packer.FromBytes<TextPayload>(Packer.ToBytes(new TextPayload { Text = "Zażółć 🚀 jaźń" }));
            var empty = Packer.FromBytes<TextPayload>(Packer.ToBytes(new TextPayload { Text = string.Empty }));
            var nullText = Packer.FromBytes<TextPayload>(Packer.ToBytes(new TextPayload { Text = null }));

            Assert.AreEqual("Zażółć 🚀 jaźń", unicode.Text);
            Assert.AreEqual(string.Empty, empty.Text);
            Assert.AreEqual(string.Empty, nullText.Text);
        }

        [Test]
        public void PayloadLargerThanStackBufferRoundTrips() {
            var large = new string('x', 12000);

            var result = Packer.FromBytes<TextPayload>(Packer.ToBytes(new TextPayload { Text = large }));

            Assert.AreEqual(large, result.Text);

            var small = Packer.FromBytes<TextPayload>(Packer.ToBytes(new TextPayload { Text = "small" }));

            Assert.AreEqual("small", small.Text);
        }

        [Test]
        public void BytesRoundTripAndNullBecomesEmpty() {
            var buffer = new byte[64];
            var writeSpan = buffer.AsSpan();

            Packer.WriteBytes(ref writeSpan, new byte[] { 1, 2, 3 });
            Packer.WriteBytes(ref writeSpan, (byte[])null);

            ReadOnlySpan<byte> readSpan = buffer;

            Assert.AreEqual(new byte[] { 1, 2, 3 }, Packer.ReadBytes(ref readSpan));
            Assert.AreEqual(Array.Empty<byte>(), Packer.ReadBytes(ref readSpan));
        }

        [Test]
        public void IntListRoundTripAndNullBecomesEmpty() {
            var buffer = new byte[64];
            var writeSpan = buffer.AsSpan();

            Packer.PackIntList(ref writeSpan, new List<int> { 7, -8, 9 });
            Packer.PackIntList(ref writeSpan, null);

            ReadOnlySpan<byte> readSpan = buffer;

            Assert.AreEqual(new List<int> { 7, -8, 9 }, Packer.UnpackIntList(ref readSpan));
            Assert.AreEqual(new List<int>(), Packer.UnpackIntList(ref readSpan));
        }

        [Test]
        public void PackerListAndArrayRoundTrip() {
            var items = new List<TextPayload> {
                new() { Text = "first" },
                new() { Text = "second" }
            };

            var listResult = Packer.UnpackListFromBytes<TextPayload>(Packer.PackListToBytes(items));

            Assert.AreEqual(2, listResult.Count);
            Assert.AreEqual("first", listResult[0].Text);
            Assert.AreEqual("second", listResult[1].Text);

            var arrayResult = Packer.UnpackArrayFromBytes<TextPayload>(Packer.PackArrayToBytes(items.ToArray()));

            Assert.AreEqual(2, arrayResult.Length);
            Assert.AreEqual("first", arrayResult[0].Text);
            Assert.AreEqual("second", arrayResult[1].Text);

            Assert.IsEmpty(Packer.UnpackListFromBytes<TextPayload>(Packer.PackListToBytes<TextPayload>(null)));
            Assert.IsEmpty(Packer.UnpackArrayFromBytes<TextPayload>(Packer.PackArrayToBytes<TextPayload>(null)));
        }

        [Test]
        public void TransformDataRoundTrips() {
            var data = new TransformData(new Vector3(10f, 20f, 30f), new Vector3(0f, 90f, 45f), 2.5f);

            var result = Packer.FromBytes<TransformData>(Packer.ToBytes(data));

            Assert.AreEqual(data.Position, result.Position);
            Assert.AreEqual(data.Rotation, result.Rotation);
            Assert.AreEqual(data.Scale, result.Scale);
        }

        [Test]
        public void AreBytesEqualHandlesNullEmptyAndContent() {
            Assert.IsTrue(Packer.AreBytesEqual(null, null));
            Assert.IsTrue(Packer.AreBytesEqual(null, Array.Empty<byte>()));
            Assert.IsTrue(Packer.AreBytesEqual(Array.Empty<byte>(), Array.Empty<byte>()));
            Assert.IsTrue(Packer.AreBytesEqual(new byte[] { 1, 2, 3 }, new byte[] { 1, 2, 3 }));
            Assert.IsFalse(Packer.AreBytesEqual(new byte[] { 1, 2, 3 }, new byte[] { 1, 2, 4 }));
            Assert.IsFalse(Packer.AreBytesEqual(new byte[] { 1, 2, 3 }, new byte[] { 1, 2 }));
            Assert.IsFalse(Packer.AreBytesEqual(null, new byte[] { 1 }));
        }
    }
}
