namespace NhsTechTest.Domain.Entities;

public sealed class Patient
{
    private Patient() { }

    public static Patient Create(
        int id,
        string nhsNumber,
        string name,
        DateTime dateOfBirth,
        string gpPractice)
    {
        if (id <= 0)
            throw new ArgumentException("Patient ID must be positive", nameof(id));

        if (string.IsNullOrWhiteSpace(nhsNumber))
            throw new ArgumentException("NHS Number is required", nameof(nhsNumber));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Patient name is required", nameof(name));

        if (dateOfBirth > DateTime.UtcNow)
            throw new ArgumentException("Date of birth cannot be in the future", nameof(dateOfBirth));

        if (string.IsNullOrWhiteSpace(gpPractice))
            throw new ArgumentException("GP Practice is required", nameof(gpPractice));

        return new Patient
        {
            Id = id,
            NHSNumber = nhsNumber,
            Name = name,
            DateOfBirth = dateOfBirth,
            GPPractice = gpPractice
        };
    }

    public int Id { get; private set; }
    public string NHSNumber { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public DateTime DateOfBirth { get; private set; }
    public string GPPractice { get; private set; } = string.Empty;
}
