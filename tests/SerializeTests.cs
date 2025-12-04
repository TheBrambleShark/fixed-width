namespace Serde.FixedWidth.Tests
{
    public sealed class SerializeTests
    {
        [Theory]
        [ClassData(typeof(TestData.SamplePeople))]
        public void Serialize_ShouldSucceed_Lines(int index, Person person)
        {
            Assert.NotNull(person);

            string line = string.Empty;
            Exception ex = Record.Exception(() => line = FixedWidthSerializer.Serialize(person));
            Assert.Null(ex);

            Assert.Equal(Constants.SampleLines[index], line);
        }

        [Fact]
        public void Serialize_ShouldSucceed_EntireDocument()
        {
            Assert.NotEmpty(Constants.SamplePeople);

            IEnumerable<string> lines = [];
            Exception ex = Record.Exception(() => lines = FixedWidthSerializer.SerializeDocument(Constants.SamplePeople));
            Assert.Null(ex);
            Assert.NotEmpty(lines);

            Assert.Equal(Constants.SampleLines, lines);
        }
    }
}
