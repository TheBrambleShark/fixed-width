using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Reflection.PortableExecutable;
using System.Text;

namespace Serde.FixedWidth.Reader
{
    internal sealed partial class FixedWidthReader : ITypeDeserializer
    {
        int? ITypeDeserializer.SizeOpt => null;
        private int _count = 0;

        T ITypeDeserializer.ReadValue<T>(ISerdeInfo info, int index, IDeserialize<T> deserialize)
            => deserialize.Deserialize(this);

        public string ReadString(ISerdeInfo typeInfo, int index)
            => GetText(typeInfo, index, out _).ToString();

        public bool ReadBool(ISerdeInfo typeInfo, int index)
        {
            var span = GetText(typeInfo, index, out var attribute);

            if (string.IsNullOrEmpty(attribute.Format))
            {
                return bool.Parse(span);
            }

            string[] splitFormat = attribute.Format.Split('/', StringSplitOptions.TrimEntries);
            if (splitFormat.Length != 2)
            {
                throw new InvalidOperationException("Split format must be an empty string or have true and false text separated by a forward slash ('/')");
            }

            if (span.Equals(splitFormat[0], StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            else if (span.Equals(splitFormat[1], StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            else
            {
                throw new InvalidOperationException($"Value '{span}' was neither '{splitFormat[0]}' nor '{splitFormat[1]}'.");
            }
        }

        public char ReadChar(ISerdeInfo typeInfo, int index)
        {
            var span = GetText(typeInfo, index, out _);
            return span.Length == 1 ? span[0] : throw new InvalidOperationException("Char field comprised of multiple non-space characters.");
        }

        public DateTime ReadDateTime(ISerdeInfo typeInfo, int index)
        {
            var span = GetText(typeInfo, index, out var attribute);
            return string.IsNullOrEmpty(attribute.Format)
                ? DateTime.Parse(span)
                : DateTime.ParseExact(span, attribute.Format, CultureInfo.CurrentCulture);
        }

        public TNumber ReadNumber<TNumber>(ISerdeInfo typeInfo, int index, NumberStyles numberStyles)
            where TNumber : struct, INumber<TNumber>
        {
            var span = GetText(typeInfo, index, out _);
            if (span.IsEmpty)
            {
                return TNumber.Zero;
            }

            if (TryReadPercentage(span, numberStyles, out TNumber value))
            {
                return value;
            }

            return TNumber.Parse(span, numberStyles, CultureInfo.InvariantCulture);

            static bool TryReadPercentage(ReadOnlySpan<char> span, NumberStyles numberStyles, out TNumber number)
            {
                number = TNumber.Zero;

                ReadOnlySpan<char> percentSymbol = CultureInfo.CurrentCulture.NumberFormat.PercentSymbol;

                if (!MemoryExtensions.Contains(span, percentSymbol, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                // We only expect to see one entry, but 2 is used because a destination length of 1 has special handling.
                Span<Range> ranges = stackalloc Range[2];
                int splitCount = span.Split(ranges, percentSymbol, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                if (splitCount != 1)
                {
                    return false;
                }

                return TNumber.TryParse(span[ranges[0]], numberStyles, CultureInfo.InvariantCulture, out number);
            }
        }

        private ReadOnlySpan<char> GetText(ISerdeInfo typeInfo, int index, out FixedFieldInfoAttribute attribute)
        {
            var customAttribute = typeInfo.GetFieldAttributes(index).FirstOrDefault(it => it.AttributeType == typeof(FixedFieldInfoAttribute));
            attribute = FixedFieldInfoAttribute.FromCustomAttributeData(customAttribute);

            return _line.AsSpan(attribute.Offset, attribute.Length).Trim();
        }

        int ITypeDeserializer.TryReadIndex(ISerdeInfo info) => TryReadIndex(info, out _);

        (int, string? errorName) ITypeDeserializer.TryReadIndexWithName(ISerdeInfo info)
        {
            int index = TryReadIndex(info, out string? errorName);
            return (index, errorName);
        }

        private int TryReadIndex(ISerdeInfo info, out string? errorName)
        {
            return info.Kind switch
            {
                InfoKind.CustomType => ReadCustomTypeIndex(info, out errorName),
                InfoKind.Enum => ReadEnumIndex(info, out errorName),
                _ => NotDefined(info, out errorName)
            };

            int ReadCustomTypeIndex(ISerdeInfo info, out string? errorName)
            {
                errorName = null;
                if (_count >= info.FieldCount)
                {
                    return ITypeDeserializer.EndOfType;
                }

                var name = info.GetFieldName(_count);
                if (name.IsEmpty)
                {
                    errorName = "Could not locate field.";
                    return ITypeDeserializer.IndexNotFound;
                }

                return _count++;
            }

            int ReadEnumIndex(ISerdeInfo info, out string? errorName)
            {
                errorName = null;
                if (_count >= info.FieldCount)
                {
                    return ITypeDeserializer.EndOfType;
                }

                return _count++;
            }

            static int NotDefined(ISerdeInfo info, out string errorName)
            {
                errorName = $"Expected a custom type or enum; found: {info.Kind}";
                return ITypeDeserializer.IndexNotFound;
            }
        }

        void ITypeDeserializer.SkipValue(ISerdeInfo info, int index)
        {
        }

        bool ITypeDeserializer.ReadBool(ISerdeInfo info, int index)
        {
            var text = GetText(info, index, out var attribute);
            if (!string.IsNullOrEmpty(attribute.Format))
            {
                string[] splitFormat = attribute.Format.Split('/', StringSplitOptions.TrimEntries);
                if (splitFormat.Length != 2)
                {
                    throw new InvalidOperationException("Split format must be an empty string or have true and false text separated by a forward slash ('/')");
                }

                if (MemoryExtensions.Equals(text, splitFormat[0], StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                if (MemoryExtensions.Equals(text, splitFormat[1], StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            if (bool.TryParse(text, out bool b))
            {
                return b;
            }

            throw new InvalidOperationException($"Expected boolean; found {text}.");
        }

        char ITypeDeserializer.ReadChar(ISerdeInfo info, int index)
        {
            var text = GetText(info, index, out _);
            if (text.Length >= 1)
            {
                return text[0];
            }

            return '\0';
        }

        byte ITypeDeserializer.ReadU8(ISerdeInfo info, int index) => ReadNumber<byte>(info, index, NumberStyles.Number);
        ushort ITypeDeserializer.ReadU16(ISerdeInfo info, int index) => ReadNumber<ushort>(info, index, NumberStyles.Number);
        uint ITypeDeserializer.ReadU32(ISerdeInfo info, int index) => ReadNumber<uint>(info, index, NumberStyles.Number);
        ulong ITypeDeserializer.ReadU64(ISerdeInfo info, int index) => ReadNumber<ulong>(info, index, NumberStyles.Number);
        sbyte ITypeDeserializer.ReadI8(ISerdeInfo info, int index) => ReadNumber<sbyte>(info, index, NumberStyles.Number);
        short ITypeDeserializer.ReadI16(ISerdeInfo info, int index) => ReadNumber<short>(info, index, NumberStyles.Number);
        int ITypeDeserializer.ReadI32(ISerdeInfo info, int index) => ReadNumber<int>(info, index, NumberStyles.Number);
        long ITypeDeserializer.ReadI64(ISerdeInfo info, int index) => ReadNumber<long>(info, index, NumberStyles.Number);
        float ITypeDeserializer.ReadF32(ISerdeInfo info, int index) => ReadNumber<float>(info, index, NumberStyles.Float);
        double ITypeDeserializer.ReadF64(ISerdeInfo info, int index) => ReadNumber<double>(info, index, NumberStyles.Float);
        decimal ITypeDeserializer.ReadDecimal(ISerdeInfo info, int index) => ReadNumber<decimal>(info, index, NumberStyles.Currency);

        string ITypeDeserializer.ReadString(ISerdeInfo info, int index)
        {
            return GetText(info, index, out _).ToString();
        }

        void ITypeDeserializer.ReadBytes(ISerdeInfo info, int index, IBufferWriter<byte> writer)
        {
            throw new NotImplementedException();
        }
    }
}
