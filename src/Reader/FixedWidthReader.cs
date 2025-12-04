using System.Buffers;
using System.Globalization;

namespace Serde.FixedWidth.Reader
{
    internal sealed partial class FixedWidthReader(string line) : IDeserializer
    {
        private readonly string _line = line;

        ITypeDeserializer IDeserializer.ReadType(ISerdeInfo typeInfo)
        {
            return typeInfo.Kind switch
            {
                InfoKind.CustomType => this,
                _ => throw new ArgumentException("Invalid type for ReadType: " + typeInfo.Kind)
            };
        }

        public void Dispose()
        {
        }

        T? IDeserializer.ReadNullableRef<T>(IDeserialize<T> deserialize) where T : class => throw new NotImplementedException();
        bool IDeserializer.ReadBool() => throw new NotImplementedException();
        char IDeserializer.ReadChar() => throw new NotImplementedException();
        byte IDeserializer.ReadU8() => throw new NotImplementedException();
        ushort IDeserializer.ReadU16() => throw new NotImplementedException();
        uint IDeserializer.ReadU32() => throw new NotImplementedException();
        ulong IDeserializer.ReadU64() => throw new NotImplementedException();
        sbyte IDeserializer.ReadI8() => throw new NotImplementedException();
        short IDeserializer.ReadI16() => throw new NotImplementedException();
        int IDeserializer.ReadI32() => throw new NotImplementedException();
        long IDeserializer.ReadI64() => throw new NotImplementedException();
        float IDeserializer.ReadF32() => throw new NotImplementedException();
        double IDeserializer.ReadF64() => throw new NotImplementedException();
        decimal IDeserializer.ReadDecimal() => throw new NotImplementedException();
        string IDeserializer.ReadString() => throw new NotImplementedException();
        DateTime IDeserializer.ReadDateTime() => throw new NotImplementedException();
        void IDeserializer.ReadBytes(IBufferWriter<byte> writer) => throw new NotImplementedException();
    }
}
