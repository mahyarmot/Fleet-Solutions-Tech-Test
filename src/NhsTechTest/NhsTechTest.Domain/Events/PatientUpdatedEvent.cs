namespace NhsTechTest.Domain.Events;

/// <summary>
/// Domain event raised when a patient is updated
/// </summary>
public sealed class PatientUpdatedEvent
{
    public int PatientId { get; init; }
    public string NHSNumber { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public DateTime DateOfBirth { get; init; }
    public string GPPractice { get; init; } = string.Empty;
    public DateTime UpdatedAt { get; init; }
    public string EventId { get; init; } = Guid.NewGuid().ToString();
    public string EventType => "PatientUpdated";
}
