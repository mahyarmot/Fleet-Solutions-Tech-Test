using System.Text.Json;
using Azure.Data.Tables;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NhsTechTest.Domain.Events;

namespace PatientAuditLogger;

/// <summary>
/// Azure Function that logs all patient events to Azure Table Storage for audit trail
/// Subscribes to the 'audit-logs' Service Bus subscription
/// </summary>
public class PatientAuditLoggerFunction(ILogger<PatientAuditLoggerFunction> logger, TableClient tableClient)
{
    private readonly ILogger<PatientAuditLoggerFunction> _logger = logger;
    private readonly TableClient _tableClient = tableClient;

    [Function("PatientAuditLogger")]
    public async Task Run(
        [ServiceBusTrigger(
            "patient-events",
            "audit-logs",
            Connection = "ServiceBusConnection")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions)
    {
        try
        {
            _logger.LogInformation($"Logging audit entry for message: {message.MessageId}");

            var eventType = message.ApplicationProperties["EventType"].ToString();
            var patientId = message.ApplicationProperties["PatientId"].ToString();

            var auditEntry = new PatientAuditEntry
            {
                PartitionKey = patientId!,
                RowKey = message.MessageId,
                EventId = message.MessageId,
                EventType = eventType!,
                PatientId = int.Parse(patientId!),
                Timestamp = DateTimeOffset.UtcNow,
                EventData = message.Body.ToString(),
                CorrelationId = message.CorrelationId
            };

            switch (eventType)
            {
                case "PatientCreated":
                    var createdEvent = JsonSerializer.Deserialize<PatientCreatedEvent>(message.Body.ToString());
                    auditEntry.PatientName = createdEvent?.Name;
                    auditEntry.NHSNumber = createdEvent?.NHSNumber;
                    auditEntry.GPPractice = createdEvent?.GPPractice;
                    auditEntry.Action = "Patient record created in system";
                    break;

                case "PatientUpdated":
                    var updatedEvent = JsonSerializer.Deserialize<PatientUpdatedEvent>(message.Body.ToString());
                    auditEntry.PatientName = updatedEvent?.Name;
                    auditEntry.NHSNumber = updatedEvent?.NHSNumber;
                    auditEntry.GPPractice = updatedEvent?.GPPractice;
                    auditEntry.Action = "Patient record updated";
                    break;

                case "PatientDeleted":
                    var deletedEvent = JsonSerializer.Deserialize<PatientDeletedEvent>(message.Body.ToString());
                    auditEntry.PatientName = deletedEvent?.Name;
                    auditEntry.NHSNumber = deletedEvent?.NHSNumber;
                    auditEntry.GPPractice = deletedEvent?.GPPractice;
                    auditEntry.Action = "Patient record deleted from system";
                    break;

                default:
                    auditEntry.Action = $"Unknown event type: {eventType}";
                    break;
            }

            // Save to Table Storage
            await _tableClient.AddEntityAsync(auditEntry);

            _logger.LogInformation($"Audit entry saved:");
            _logger.LogInformation($"   Patient ID: {auditEntry.PatientId}");
            _logger.LogInformation($"   Event Type: {auditEntry.EventType}");
            _logger.LogInformation($"   Action: {auditEntry.Action}");
            _logger.LogInformation($"   Timestamp: {auditEntry.Timestamp}");

            // Complete the message
            await messageActions.CompleteMessageAsync(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error logging audit entry for message {message.MessageId}: {ex.Message}");

            await messageActions.DeadLetterMessageAsync(
                message,
                deadLetterReason: "AuditLoggingError",
                deadLetterErrorDescription: ex.Message);
        }
    }
}
