using ErrorOr;
using FluentAssertions;
using Moq;
using NhsTechTest.Application.Patients.DTOs;
using NhsTechTest.Application.Patients.Queries;
using NhsTechTest.Domain.Entities;
using NhsTechTest.Domain.Repositories;

namespace NhsTechTest.Application.Tests.Queries.Patients;

public class GetPatientQueryHandlerTests
{
    private readonly Mock<IPatientRepository> _mockRepository;
    private readonly GetPatientQueryHandler _handler;

    public GetPatientQueryHandlerTests()
    {
        _mockRepository = new Mock<IPatientRepository>();
        _handler = new GetPatientQueryHandler(_mockRepository.Object);
    }

    [Fact]
    public async Task Handle_WhenPatientExists_ReturnsPatientSummary()
    {
        // Arrange
        var patientId = 1;
        var patient = Patient.Create(
            id: patientId,
            nhsNumber: "485 777 3456",
            name: "Sarah Johnson",
            dateOfBirth: new DateTime(1985, 3, 15),
            gpPractice: "Riverside Medical Centre");

        _mockRepository
            .Setup(r => r.GetByIdAsync(patientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);

        var query = new GetPatientQuery(patientId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeOfType<PatientSummaryDto>();
        result.Value.Id.Should().Be(patientId);
        result.Value.Name.Should().Be("Sarah Johnson");
        result.Value.NHSNumber.Should().Be("485 777 3456");
        result.Value.GPPractice.Should().Be("Riverside Medical Centre");
    }

    [Fact]
    public async Task Handle_WhenPatientNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var patientId = 999;
        _mockRepository
            .Setup(r => r.GetByIdAsync(patientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Patient?)null);

        var query = new GetPatientQuery(patientId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        result.FirstError.Code.Should().Be("Patient.NotFound");
        result.FirstError.Description.Should().Contain(patientId.ToString());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task Handle_WhenInvalidPatientId_ReturnsValidationError(int invalidId)
    {
        // Arrange
        var query = new GetPatientQuery(invalidId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Validation);
        result.FirstError.Code.Should().Be("Patient.InvalidId");
        result.FirstError.Description.Should().Contain("positive number");
    }

    [Fact]
    public async Task Handle_WhenCancellationRequested_PropagatesCancellation()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        _mockRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        var query = new GetPatientQuery(1);

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _handler.Handle(query, cts.Token));
    }

    [Fact]
    public async Task Handle_MapsAllPatientPropertiesCorrectly()
    {
        // Arrange
        var dateOfBirth = new DateTime(1990, 6, 15);
        var patient = Patient.Create(
            id: 42,
            nhsNumber: "123 456 7890",
            name: "Test Patient",
            dateOfBirth: dateOfBirth,
            gpPractice: "Test Practice");

        _mockRepository
            .Setup(r => r.GetByIdAsync(42, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);

        var query = new GetPatientQuery(42);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        var dto = result.Value;
        dto.Id.Should().Be(42);
        dto.NHSNumber.Should().Be("123 456 7890");
        dto.Name.Should().Be("Test Patient");
        dto.DateOfBirth.Should().Be(dateOfBirth);
        dto.GPPractice.Should().Be("Test Practice");
    }

    [Fact]
    public async Task Handle_CallsRepositoryWithCorrectParameters()
    {
        // Arrange
        var patientId = 5;
        var cancellationToken = new CancellationToken();
        _mockRepository
            .Setup(r => r.GetByIdAsync(patientId, cancellationToken))
            .ReturnsAsync((Patient?)null);

        var query = new GetPatientQuery(patientId);

        // Act
        await _handler.Handle(query, cancellationToken);

        // Assert
        _mockRepository.Verify(
            r => r.GetByIdAsync(patientId, cancellationToken),
            Times.Once);
    }
}
