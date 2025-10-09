using System;

namespace SpellBound.Core {
    /// <summary>
    /// 
    /// </summary>
    public struct SbbData : IPacker {
        public string Id;
        public byte[] PackedData;
        
        public override string ToString() => 
                $"SbbData ▶ Id={Id}, Bytes={PackedData?.Length ?? 0}";
        
        public void Pack(ref Span<byte> buffer) {
            var writer = new PackerWriter(buffer);

            writer.Write(Id).WriteBytes(PackedData);
            
            buffer = writer.Remaining;
        }
        
        public void Unpack(ref ReadOnlySpan<byte> buffer) {
            var reader = new PackerReader(buffer);

            Id = reader.ReadString();
            PackedData = reader.ReadBytes().ToArray();

            buffer = reader.Remaining;
        }
    }
}   
