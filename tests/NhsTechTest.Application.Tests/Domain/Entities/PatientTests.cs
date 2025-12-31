using FluentAssertions;
using NhsTechTest.Domain.Entities;

namespace NhsTechTest.Application.Tests.Domain.Entities;

public class PatientTests
{
    [Fact]
    public void Create_WithValidData_CreatesPatient()
    {
        // Arrange
        var id = 1;
        var nhsNumber = "485 777 3456";
        var name = "Sarah Johnson";
        var dateOfBirth = new DateTime(1985, 3, 15);
        var gpPractice = "Riverside Medical Centre";

        // Act
        var patient = Patient.Create(id, nhsNumber, name, dateOfBirth, gpPractice);

        // Assert
        patient.Should().NotBeNull();
        patient.Id.Should().Be(id);
        patient.NHSNumber.Should().Be(nhsNumber);
        patient.Name.Should().Be(name);
        patient.DateOfBirth.Should().Be(dateOfBirth);
        patient.GPPractice.Should().Be(gpPractice);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Create_WithInvalidId_ThrowsArgumentException(int invalidId)
    {
        // Act
        var act = () => Patient.Create(
            invalidId,
            "123 456 7890",
            "Test Patient",
            DateTime.UtcNow.AddYears(-30),
            "Test Practice");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Patient ID must be positive*");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithInvalidNHSNumber_ThrowsArgumentException(string? invalidNhs)
    {
        // Act
        var act = () => Patient.Create(
            1,
            invalidNhs!,
            "Test Patient",
            DateTime.UtcNow.AddYears(-30),
            "Test Practice");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*NHS Number is required*");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithInvalidName_ThrowsArgumentException(string? invalidName)
    {
        // Act
        var act = () => Patient.Create(
            1,
            "123 456 7890",
            invalidName!,
            DateTime.UtcNow.AddYears(-30),
            "Test Practice");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Patient name is required*");
    }

    [Fact]
    public void Create_WithFutureDateOfBirth_ThrowsArgumentException()
    {
        // Arrange
        var futureDate = DateTime.UtcNow.AddDays(1);

        // Act
        var act = () => Patient.Create(
            1,
            "123 456 7890",
            "Test Patient",
            futureDate,
            "Test Practice");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Date of birth cannot be in the future*");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithInvalidGPPractice_ThrowsArgumentException(string? invalidGp)
    {
        // Act
        var act = () => Patient.Create(
            1,
            "123 456 7890",
            "Test Patient",
            DateTime.UtcNow.AddYears(-30),
            invalidGp!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*GP Practice is required*");
    }
}
