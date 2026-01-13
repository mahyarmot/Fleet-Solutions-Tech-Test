namespace NhsTechTest.Domain.Events;

/// <summary>
/// Domain event raised when a new patient is created
/// </summary>
public sealed class PatientCreatedEvent
{
    public int PatientId { get; init; }
    public string NHSNumber { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public DateTime DateOfBirth { get; init; }
    public string GPPractice { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public string EventId { get; init; } = Guid.NewGuid().ToString();
    public string EventType => "PatientCreated";
}