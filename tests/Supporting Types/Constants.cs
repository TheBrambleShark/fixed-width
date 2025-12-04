namespace Serde.FixedWidth.Tests
{
    public static class Constants
    {
        public const string SampleInput = """
            John      Doe       1234501011970
            Janette   Doe       6789001011969
            """;

        public static IReadOnlyList<string> SampleLines => SampleInput.Split(Environment.NewLine);

        public static IReadOnlyList<Person> SamplePeople =>
        [
            new Person("John", "Doe", 12345, new(1970, 1, 1)),
            new Person("Janette", "Doe", 67890, new(1969, 1, 1))
        ];
    }
}
