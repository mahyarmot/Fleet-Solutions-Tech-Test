namespace NhsTechTest.Domain.Events;

/// <summary>
/// Domain event raised when a patient is deleted
/// </summary>
public sealed class PatientDeletedEvent
{
    public int PatientId { get; init; }
    public string NHSNumber { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string GPPractice { get; init; } = string.Empty;
    public DateTime DeletedAt { get; init; }
    public string EventId { get; init; } = Guid.NewGuid().ToString();
    public string EventType => "PatientDeleted";
}