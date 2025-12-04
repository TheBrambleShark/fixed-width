namespace Serde.FixedWidth.Tests
{
    [GenerateSerde]
    public partial record class Person
    (
        [property: FixedFieldInfo(0, 10)] string FirstName,
        [property: FixedFieldInfo(10, 10)] string LastName,
        [property: FixedFieldInfo(20, 5)] int EmployeeId,
        [property: FixedFieldInfo(25, 3)] int Age,
        [property: FixedFieldInfo(28, 8, format: "MMddyyyy")] DateTime Birthday
    );
}
