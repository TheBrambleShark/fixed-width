using System.Diagnostics.Contracts;
using System.Text;

namespace Serde.FixedWidth
{
    internal sealed class FixedWidthBuffer(StringBuilder stringBuilder)
    {
        private readonly StringBuilder _sb = stringBuilder;
        private readonly List<BufferedField> _fields = [];

        public void WriteField(string value, FixedFieldInfoAttribute attribute)
        {
            _fields.Add(new(attribute.Offset, value));
        }

        [Pure]
        public string GetText()
        {
            IOrderedEnumerable<BufferedField> _orderedFields = _fields.OrderBy(it => it.Offset);
            int index = 0;

            foreach (var field in _orderedFields)
            {
                if (field.Offset < index)
                {
                    throw new InvalidOperationException("Overflowed field length!");
                }

                index = index += field.Length;
                _sb.Append(field.Value);
            }

            return _sb.ToString();
        }
    }

    internal readonly record struct BufferedField(int Offset, string Value)
    {
        public int Length => Value.Length;
    }
}
