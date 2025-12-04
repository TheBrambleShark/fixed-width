using System.Numerics;
using System.Text;

namespace Serde.FixedWidth.Writer
{
    internal sealed partial class FixedWidthWriter : ISerializer
    {
        private readonly StringBuilder _sb;
        private readonly EnumSerializer _enumSerializer;

        public FixedWidthWriter()
        {
            _sb = new StringBuilder();
            _enumSerializer = new EnumSerializer(this);
        }

        public void WriteBool(bool b) => _sb.Append(b);

        public void WriteBytes(ReadOnlyMemory<byte> bytes) => _sb.Append(Encoding.UTF8.GetString(bytes.Span));

        public void WriteChar(char c) => _sb.Append(c);

        ITypeSerializer ISerializer.WriteCollection(ISerdeInfo info, int? count) => throw new NotImplementedException();

        public void WriteDateTime(DateTime dt) => _sb.Append(dt.ToString());

        public void WriteDateTimeOffset(DateTimeOffset dt) => _sb.Append(dt.ToString());

        public void WriteDecimal(decimal d) => _sb.Append(d);
        public void WriteF32(float f) => _sb.Append(f);
        public void WriteF64(double d) => _sb.Append(d);
        public void WriteI16(short i16) => _sb.Append(i16);
        public void WriteI32(int i32) => _sb.Append(i32);
        public void WriteI64(long i64) => _sb.Append(i64);
        public void WriteI8(sbyte b) => _sb.Append(b);
        public void WriteU16(ushort u16) => _sb.Append(u16);
        public void WriteU32(uint u32) => _sb.Append(u32);
        public void WriteU64(ulong u64) => _sb.Append(u64);
        public void WriteU8(byte b) => _sb.Append(b);

        public void WriteNull()
        {
        }

        public void WriteString(string s) => _sb.Append(s);

        ITypeSerializer ISerializer.WriteType(ISerdeInfo info)
        {
            return info.Kind switch
            {
                InfoKind.CustomType => this,
                InfoKind.Enum => _enumSerializer,
                _ => throw new InvalidOperationException($"Unexpected info kind: {info.Kind}")
            };
        }
        public override string ToString()
        {
            return _sb.ToString();
        }
    }
}
