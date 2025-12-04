namespace Serde.FixedWidth.Tests
{
    public sealed class DeserializeTests
    {
        [Theory]
        [ClassData(typeof(TestData.SampleFileLines))]
        public void Deserialize_SingleLines(int index, string line)
        {
            Assert.False(string.IsNullOrEmpty(line));

            Assert.NotEqual(string.Empty, line);

            Person person = default!;

            Exception ex = Record.Exception(() => person = FixedWidthSerializer.Deserialize<Person>(line));
            Assert.Null(ex);

            Assert.Equal(Constants.SamplePeople[index], person);
        }

        [Theory]
        [ClassData(typeof(TestData.SampleFile))]
        public void Deserialize_File(string file)
        {
            Assert.NotEqual(string.Empty, file);
            IEnumerable<Person> people = [];

            Exception ex = Record.Exception(() => people = FixedWidthSerializer.DeserializeDocument<Person>(file));
            Assert.Null(ex);

            Assert.NotEmpty(people);

            Assert.Collection(
                people,
                p1 => AssertCorrectPerson(Constants.SamplePeople[0], p1),
                p2 => AssertCorrectPerson(Constants.SamplePeople[1], p2)
            );
        }

        [Theory]
        [ClassData(typeof(TestData.SampleFileLines))]
        public void Deserialize_PartialDataThrowsAOR_Lines(int index, string line)
        {
            Assert.False(string.IsNullOrEmpty(line));

            string brokenLine = line[..5];
            Assert.Throws<ArgumentOutOfRangeException>(() => FixedWidthSerializer.Deserialize<Person>(brokenLine));
        }

        private static void AssertCorrectPerson(Person expected, Person actual)
        {
            Assert.NotNull(expected);
            Assert.NotNull(actual);
            Assert.Equal(expected, actual);
        }
    }
}
