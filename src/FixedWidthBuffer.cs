using System.Diagnostics.Contracts;
using System.Text;

namespace Serde.FixedWidth
{
    internal sealed class FixedWidthBuffer
    {
        private readonly List<BufferedField> _fields = [];

        public void WriteField(string value, FixedFieldInfoAttribute attribute)
            => WriteField(value, attribute.Offset);

        public void WriteField(string value, int offset)
        {
            _fields.Add(new(offset, value));
        }

        [Pure]
        public string GetText()
        {
            StringBuilder sb = new();
            IOrderedEnumerable<BufferedField> _orderedFields = _fields.OrderBy(it => it.Offset);
            int index = 0;

            foreach (var field in _orderedFields)
            {
                if (field.Offset < index)
                {
                    throw new InvalidOperationException("Overflowed field length!");
                }

                index = index += field.Length;
                sb.Append(field.Value);
            }

            return sb.ToString();
        }
    }

    internal readonly record struct BufferedField(int Offset, string Value)
    {
        public int Length => Value.Length;
    }
}
