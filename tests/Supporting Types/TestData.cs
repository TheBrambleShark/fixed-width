namespace Serde.FixedWidth.Tests
{
    public static class TestData
    {
        public sealed class SampleFile : TheoryData<string>
        {
            public SampleFile()
            {
                Add(Constants.SampleInput);
            }
        }

        public sealed class SampleFileLines : TheoryData<int, string>
        {
            public SampleFileLines()
            {
                for (int i = 0; i < Constants.SampleLines.Count; i++)
                {
                    Add(i, Constants.SampleLines[i]);
                }
            }
        }

        public sealed class SamplePeople : TheoryData<int, Person>
        {
            public SamplePeople()
            {
                for (int i = 0; i < Constants.SamplePeople.Count; i++)
                {
                    Add(i, Constants.SamplePeople[i]);
                }
            }
        }
    }
}
