using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NhsTechTest.Domain.Events;

namespace PatientEmailNotifier;

/// <summary>
/// Azure Function that sends email notifications for patient events
/// Subscribes to the 'email-notifications' Service Bus subscription
/// </summary>
public class PatientEmailNotifierFunction(ILogger<PatientEmailNotifierFunction> logger)
{
    private readonly ILogger<PatientEmailNotifierFunction> _logger = logger;

    [Function("PatientEmailNotifier")]
    public async Task Run(
        [ServiceBusTrigger(
            "patient-events",
            "email-notifications",
            Connection = "ServiceBusConnection")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions)
    {
        try
        {
            _logger.LogInformation($"Processing message: {message.MessageId}");
            _logger.LogInformation($"Event Type: {message.Subject}");
            _logger.LogInformation($"Correlation ID: {message.CorrelationId}");

            var eventType = message.ApplicationProperties["EventType"].ToString();

            switch (eventType)
            {
                case "PatientCreated":
                    await HandlePatientCreated(message);
                    break;
                case "PatientUpdated":
                    await HandlePatientUpdated(message);
                    break;
                case "PatientDeleted":
                    await HandlePatientDeleted(message);
                    break;
                default:
                    _logger.LogWarning($"Unknown event type: {eventType}");
                    break;
            }

            await messageActions.CompleteMessageAsync(message);

            _logger.LogInformation($"Message {message.MessageId} processed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error processing message {message.MessageId}: {ex.Message}");

            await messageActions.DeadLetterMessageAsync(
                message,
                deadLetterReason: "ProcessingError",
                deadLetterErrorDescription: ex.Message);
        }
    }

    private async Task HandlePatientCreated(ServiceBusReceivedMessage message)
    {
        var patientEvent = JsonSerializer.Deserialize<PatientCreatedEvent>(
            message.Body.ToString());

        if (patientEvent == null)
        {
            _logger.LogWarning("Failed to deserialize PatientCreatedEvent");
            return;
        }

        _logger.LogInformation($"Sending welcome email to patient: {patientEvent.Name}");
        _logger.LogInformation($"   NHS Number: {patientEvent.NHSNumber}");
        _logger.LogInformation($"   GP Practice: {patientEvent.GPPractice}");

        await SendEmailAsync();

        _logger.LogInformation("Welcome email sent successfully!");
    }

    private async Task HandlePatientUpdated(ServiceBusReceivedMessage message)
    {
        var patientEvent = JsonSerializer.Deserialize<PatientUpdatedEvent>(
            message.Body.ToString());

        if (patientEvent == null)
        {
            _logger.LogWarning("Failed to deserialize PatientUpdatedEvent");
            return;
        }

        _logger.LogInformation($"Sending update notification to patient: {patientEvent.Name}");
        _logger.LogInformation($"   NHS Number: {patientEvent.NHSNumber}");

        await SendEmailAsync();

        _logger.LogInformation("Update notification sent successfully!");
    }

    private async Task HandlePatientDeleted(ServiceBusReceivedMessage message)
    {
        var patientEvent = JsonSerializer.Deserialize<PatientDeletedEvent>(
            message.Body.ToString());

        if (patientEvent == null)
        {
            _logger.LogWarning("Failed to deserialize PatientDeletedEvent");
            return;
        }

        _logger.LogInformation($"Sending account closure notification to patient: {patientEvent.Name}");
        _logger.LogInformation($"   NHS Number: {patientEvent.NHSNumber}");

        await SendEmailAsync();

        _logger.LogInformation("Closure notification sent successfully!");
    }

    private static async Task SendEmailAsync()
    {
        // Simulate email sending
        await Task.Delay(100);
    }
}
