namespace Serde.FixedWidth.Tests
{
    [GenerateSerde]
    public partial record class Person
    (
        [property: FixedFieldInfo(0, 10)] string FirstName,
        [property: FixedFieldInfo(10, 10)] string LastName,
        [property: FixedFieldInfo(20, 5)] int EmployeeId,
        [property: FixedFieldInfo(25, 8, format: "MMddyyyy")] DateTime Birthday
    )
    {
        // There seems to be a bug where the Serde source generator attempts to assign a value to readonly fields,
        // even though it should be ignored. Commenting it out for now.
        /*
        [FixedFieldInfo(25, 3, "D3")]
        public int Age => DateOnly.FromDateTime(DateTime.Today).Year - Birthday.Year;
        */
    }
}
