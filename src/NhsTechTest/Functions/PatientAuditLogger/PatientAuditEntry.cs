using Azure;
using Azure.Data.Tables;


namespace PatientAuditLogger;

/// <summary>
/// Audit entry entity for Azure Table Storage
/// </summary>
public class PatientAuditEntry : ITableEntity
{
    public string PartitionKey { get; set; } = string.Empty; // PatientId
    public string RowKey { get; set; } = string.Empty; // EventId
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public string EventId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public int PatientId { get; set; }
    public string? PatientName { get; set; }
    public string? NHSNumber { get; set; }
    public string? GPPractice { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EventData { get; set; } = string.Empty;
    public string? CorrelationId { get; set; }
}
