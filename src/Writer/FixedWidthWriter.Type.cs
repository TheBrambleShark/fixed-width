using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Text;

namespace Serde.FixedWidth.Writer
{
    internal sealed partial class FixedWidthWriter : ITypeSerializer
    {
        private const char Padding = ' ';

        private static FixedFieldInfoAttribute GetAttribute(ISerdeInfo typeInfo, int index)
        {
            var customAttribute = typeInfo.GetFieldAttributes(index).FirstOrDefault(it => it.AttributeType == typeof(FixedFieldInfoAttribute));
            return FixedFieldInfoAttribute.FromCustomAttributeData(customAttribute);
        }

        private void WriteText(string value, FixedFieldInfoAttribute attribute)
        {
            if (value.Length > attribute.Length)
            {
                value = attribute.OverflowHandling switch
                {
                    FieldOverflowHandling.Throw => ThrowValueTooLongForFieldException<string>(value, attribute.Length),
                    FieldOverflowHandling.Truncate => value[..attribute.Length],
                    _ => throw new ArgumentOutOfRangeException(nameof(attribute), $"{attribute.OverflowHandling} is not a valid value for {nameof(FieldOverflowHandling)}.")
                }; 
            }

            _buffer.WriteField(value.PadRight(attribute.Length), attribute);
        }

        public void End(ISerdeInfo info)
        {
        }

        public void WriteBool(ISerdeInfo typeInfo, int index, bool b)
        {
            var attribute = GetAttribute(typeInfo, index);
            if (!string.IsNullOrEmpty(attribute.Format))
            {
                string[] splitFormat = attribute.Format.Split('/', StringSplitOptions.TrimEntries);
                if (splitFormat.Length != 2)
                {
                    throw new InvalidOperationException("Split format must be an empty string or have true and false text separated by a forward slash ('/')");
                }

                WriteText(b ? splitFormat[0] : splitFormat[1], attribute);
                return;
            }

            if (b && attribute.Length == 4)
            {
                WriteText(b.ToString(), attribute);
                return;
            }
            if (!b && attribute.Length == 5)
            {
                WriteText(b.ToString(), attribute);
                return;
            }

            ThrowValueTooLongForFieldException<string>(b.ToString(), attribute.Length);
        }

        public void WriteBytes(ISerdeInfo typeInfo, int index, ReadOnlyMemory<byte> bytes)
        {
            var attribute = GetAttribute(typeInfo, index);
            WriteText(Encoding.UTF8.GetString(bytes.Span), attribute);
        }

        public void WriteChar(ISerdeInfo typeInfo, int index, char c)
        {
            var attribute = GetAttribute(typeInfo, index);
            WriteText(c.ToString(), attribute);
        }

        public void WriteDateTime(ISerdeInfo typeInfo, int index, DateTime dt)
        {
            var attribute = GetAttribute(typeInfo, index);
            string value = string.IsNullOrWhiteSpace(attribute.Format)
                ? dt.ToString()
                : dt.ToString(attribute.Format);

            WriteText(value, attribute);
        }

        public void WriteDateTimeOffset(ISerdeInfo typeInfo, int index, DateTimeOffset dt)
        {
            var attribute = GetAttribute(typeInfo, index);
            string value = string.IsNullOrWhiteSpace(attribute.Format)
                ? dt.ToString()
                : dt.ToString(attribute.Format);
            WriteText(value, attribute);
        }

        private void WriteNumber<T>(ISerdeInfo typeInfo, int index, T value)
            where T : struct, INumber<T>
        {
            var attribute = GetAttribute(typeInfo, index);

            string format = string.IsNullOrEmpty(attribute.Format)
                ? $"D{attribute.Length}"
                : attribute.Format;

            string text = value.ToString(format, CultureInfo.CurrentCulture);
            WriteText(text, attribute);
        }

        public void WriteDecimal(ISerdeInfo typeInfo, int index, decimal d) => WriteNumber(typeInfo, index, d);
        public void WriteF32(ISerdeInfo typeInfo, int index, float f) => WriteNumber(typeInfo, index, f);
        public void WriteF64(ISerdeInfo typeInfo, int index, double d) => WriteNumber(typeInfo, index, d);
        public void WriteI16(ISerdeInfo typeInfo, int index, short i16) => WriteNumber(typeInfo, index, i16);
        public void WriteI32(ISerdeInfo typeInfo, int index, int i32) => WriteNumber(typeInfo, index, i32);
        public void WriteI64(ISerdeInfo typeInfo, int index, long i64) => WriteNumber(typeInfo, index, i64);
        public void WriteI8(ISerdeInfo typeInfo, int index, sbyte b) => WriteNumber(typeInfo, index, b);
        public void WriteU16(ISerdeInfo typeInfo, int index, ushort u16) => WriteNumber(typeInfo, index, u16);
        public void WriteU32(ISerdeInfo typeInfo, int index, uint u32) => WriteNumber(typeInfo, index, u32);
        public void WriteU64(ISerdeInfo typeInfo, int index, ulong u64) => WriteNumber(typeInfo, index, u64);
        public void WriteU8(ISerdeInfo typeInfo, int index, byte b) => WriteNumber(typeInfo, index, b);
        public void WriteNull(ISerdeInfo typeInfo, int index)
        {
        }

        public void WriteString(ISerdeInfo typeInfo, int index, string s)
        {
            var attribute = GetAttribute(typeInfo, index);
            WriteText(s.PadRight(attribute.Length), attribute);
        }

        public void WriteValue<T>(ISerdeInfo typeInfo, int index, T value, ISerialize<T> serialize)
            where T : class?
        {
            serialize.Serialize(value, this);
        }

        [DoesNotReturn]
        private static T ThrowValueTooLongForFieldException<T>(string value, int maxLength)
        {
            throw new InvalidOperationException($"Cannot write {value} (length {value.Length}) to a field that is only {maxLength} long.");
        }
    }
}
