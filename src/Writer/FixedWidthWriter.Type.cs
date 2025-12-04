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

        private void PadToOffset(ISerdeInfo typeInfo, int index, out FixedFieldInfoAttribute attribute)
        {
            attribute = GetAttribute(typeInfo, index);
            PadToOffset(attribute.Offset);
        }

        private void PadToOffset(int offset)
        {
            if (_sb.Length > offset)
            {
                throw new InvalidOperationException("Overflowed field length!");
            }

            if (_sb.Length < offset)
            {
                _sb.Append(Padding, offset - _sb.Length);
            }
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

            WriteString(value.PadRight(attribute.Length));
        }

        public void End(ISerdeInfo info)
        {
        }

        public void WriteBool(ISerdeInfo typeInfo, int index, bool b)
        {
            PadToOffset(typeInfo, index, out var attribute);
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
                WriteBool(b);
                return;
            }
            if (!b && attribute.Length == 5)
            {
                WriteBool(b);
                return;
            }

            ThrowValueTooLongForFieldException<string>(b.ToString(), attribute.Length);
        }

        public void WriteBytes(ISerdeInfo typeInfo, int index, ReadOnlyMemory<byte> bytes)
        {
            var attribute = GetAttribute(typeInfo, index);
            PadToOffset(attribute.Offset);
            WriteText(Encoding.UTF8.GetString(bytes.Span), attribute);
        }

        public void WriteChar(ISerdeInfo typeInfo, int index, char c)
        {
            PadToOffset(typeInfo, index, out _);
            WriteChar(c);
        }

        public void WriteDateTime(ISerdeInfo typeInfo, int index, DateTime dt)
        {
            PadToOffset(typeInfo, index, out var attribute);
            if (!string.IsNullOrWhiteSpace(attribute.Format))
            {
                WriteText(dt.ToString(attribute.Format), attribute);
                return;
            }

            string value = dt.ToString();

            if (value.Length > attribute.Length)
            {
                ThrowValueTooLongForFieldException<string>(value, attribute.Length);
            }

            WriteDateTime(dt);
        }

        public void WriteDateTimeOffset(ISerdeInfo typeInfo, int index, DateTimeOffset dt)
        {
            PadToOffset(typeInfo, index, out var attribute);
            if (!string.IsNullOrWhiteSpace(attribute.Format))
            {
                WriteString(dt.ToString(attribute.Format));
                return;
            }

            string value = dt.ToString();
            if (value.Length > attribute.Length)
            {
                ThrowValueTooLongForFieldException<string>(value, attribute.Length);
            }

            WriteDateTimeOffset(dt);
        }

        private void WriteNumber<T>(ISerdeInfo typeInfo, int index, T value)
            where T : struct, INumber<T>
        {
            PadToOffset(typeInfo, index, out var attribute);

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
            PadToOffset(typeInfo, index, out var attribute);
            WriteText(s.PadRight(attribute.Length), attribute);
        }

        public void WriteValue<T>(ISerdeInfo typeInfo, int index, T value, ISerialize<T> serialize)
            where T : class?
        {
            PadToOffset(typeInfo, index, out _);
            serialize.Serialize(value, this);
        }

        [DoesNotReturn]
        private static T ThrowValueTooLongForFieldException<T>(string value, int maxLength)
        {
            throw new InvalidOperationException($"Cannot write {value} (length {value.Length}) to a field that is only {maxLength} long.");
        }
    }
}
