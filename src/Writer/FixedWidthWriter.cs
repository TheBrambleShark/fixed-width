using System.Numerics;
using System.Text;

namespace Serde.FixedWidth.Writer
{
    internal sealed partial class FixedWidthWriter : ISerializer
    {
        private readonly FixedWidthBuffer _buffer;
        private int _index = 0;

        public FixedWidthWriter()
        {
            _buffer = new FixedWidthBuffer();
        }

        void ISerializer.WriteBool(bool b) => WriteString(b.ToString());
        void ISerializer.WriteBytes(ReadOnlyMemory<byte> bytes) => WriteString(Encoding.UTF8.GetString(bytes.Span));
        void ISerializer.WriteChar(char c) => WriteString(c.ToString());
        ITypeSerializer ISerializer.WriteCollection(ISerdeInfo info, int? count) => throw new NotImplementedException();
        void ISerializer.WriteDateTime(DateTime dt) => WriteString(dt.ToString());
        void ISerializer.WriteDateTimeOffset(DateTimeOffset dt) => WriteString(dt.ToString());
        void ISerializer.WriteDecimal(decimal d) => WriteNumber(d);
        void ISerializer.WriteF32(float f) => WriteNumber(f);
        void ISerializer.WriteF64(double d) => WriteNumber(d);
        void ISerializer.WriteI16(short i16) => WriteNumber(i16);
        void ISerializer.WriteI32(int i32) => WriteNumber(i32);
        void ISerializer.WriteI64(long i64) => WriteNumber(i64);
        void ISerializer.WriteI8(sbyte b) => WriteNumber(b);
        void ISerializer.WriteU16(ushort u16) => WriteNumber(u16);
        void ISerializer.WriteU32(uint u32) => WriteNumber(u32);
        void ISerializer.WriteU64(ulong u64) => WriteNumber(u64);
        void ISerializer.WriteU8(byte b) => WriteNumber(b);

        private void WriteNumber<TNumber>(TNumber number)
            where TNumber : struct, INumber<TNumber>
        {
            WriteString(number.ToString() ?? string.Empty);
        }

        void ISerializer.WriteNull()
        {
        }

        public void WriteString(string s)
        {
            _index += s.Length;
            _buffer.WriteField(s, _index);
        }

        ITypeSerializer ISerializer.WriteType(ISerdeInfo info)
        {
            return info.Kind switch
            {
                InfoKind.CustomType => this,
                _ => throw new InvalidOperationException($"Unexpected info kind: {info.Kind}")
            };
        }
        public override string ToString()
        {
            return _buffer.GetText();
        }
    }
}
