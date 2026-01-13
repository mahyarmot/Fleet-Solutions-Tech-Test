using System.Text.Json;
using Azure.Messaging.EventGrid;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NhsTechTest.Domain.Events;

namespace PatientEventsProcessor;

/// <summary>
/// Azure Function that processes CosmosDB Change Feed and publishes events to Event Grid and Service Bus
/// </summary>
public class PatientEventsProcessorFunction(
    ILogger<PatientEventsProcessorFunction> logger,
    EventGridPublisherClient eventGridClient,
    ServiceBusClient serviceBusClient,
    IConfiguration configuration)
{
    private readonly ILogger<PatientEventsProcessorFunction> _logger = logger;
    private readonly EventGridPublisherClient _eventGridClient = eventGridClient;
    private readonly ServiceBusClient _serviceBusClient = serviceBusClient;
    private readonly string _topicName = configuration["ServiceBusTopicName"] ?? "patient-events";

    [Function("PatientEventsProcessor")]
    public async Task Run(
        [CosmosDBTrigger(
            databaseName: "PatientsDb",
            containerName: "PatientsContainer",
            Connection = "CosmosDbConnection",
            LeaseContainerName = "leases",
            CreateLeaseContainerIfNotExists = false)]
        IReadOnlyList<dynamic> documents)
    {
        if (documents == null || documents.Count == 0)
        {
            return;
        }

        _logger.LogInformation($"Processing {documents.Count} document(s) from CosmosDB Change Feed");

        foreach (var document in documents)
        {
            try
            {
                var eventType = DetermineEventType(document);

                switch (eventType)
                {
                    case "Created":
                        await HandlePatientCreated(document);
                        break;
                    case "Updated":
                        await HandlePatientUpdated(document);
                        break;
                    case "Deleted":
                        await HandlePatientDeleted(document);
                        break;
                    default:
                        _logger.LogWarning($"Unknown event type for document");
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing document: {ex.Message}");
            }
        }
    }

    private string DetermineEventType(dynamic document)
    {
        try
        {
            DateTime createdAt = document.createdAt;
            DateTime updatedAt = document.updatedAt;

            return createdAt == updatedAt ? "Created" : "Updated";
        }
        catch
        {
            return "Updated";
        }
    }

    private async Task HandlePatientCreated(dynamic document)
    {
        var patientEvent = new PatientCreatedEvent
        {
            PatientId = (int)document.patientId,
            NHSNumber = (string)document.nhsNumber,
            Name = (string)document.name,
            DateOfBirth = (DateTime)document.dateOfBirth,
            GPPractice = (string)document.gpPractice,
            CreatedAt = (DateTime)document.createdAt
        };

        _logger.LogInformation($"Publishing PatientCreated event for patient {patientEvent.PatientId}");

        await PublishToEventGrid(patientEvent);

        await PublishToServiceBus(patientEvent);
    }

    private async Task HandlePatientUpdated(dynamic document)
    {
        var patientEvent = new PatientUpdatedEvent
        {
            PatientId = (int)document.patientId,
            NHSNumber = (string)document.nhsNumber,
            Name = (string)document.name,
            DateOfBirth = (DateTime)document.dateOfBirth,
            GPPractice = (string)document.gpPractice,
            UpdatedAt = (DateTime)document.updatedAt
        };

        _logger.LogInformation($"Publishing PatientUpdated event for patient {patientEvent.PatientId}");

        await PublishToEventGrid(patientEvent);

        await PublishToServiceBus(patientEvent);
    }

    private async Task HandlePatientDeleted(dynamic document)
    {
        var patientEvent = new PatientDeletedEvent
        {
            PatientId = (int)document.patientId,
            NHSNumber = (string)document.nhsNumber,
            Name = (string)document.name,
            GPPractice = (string)document.gpPractice,
            DeletedAt = DateTime.UtcNow
        };

        _logger.LogInformation($"Publishing PatientDeleted event for patient {patientEvent.PatientId}");

        await PublishToEventGrid(patientEvent);

        await PublishToServiceBus(patientEvent);
    }

    private async Task PublishToEventGrid<T>(T domainEvent) where T : class
    {
        try
        {
            var eventGridEvent = new EventGridEvent(
                subject: $"/patients/{GetPatientId(domainEvent)}",
                eventType: GetEventType(domainEvent),
                dataVersion: "1.0",
                data: domainEvent);

            await _eventGridClient.SendEventAsync(eventGridEvent);

            _logger.LogInformation($"Published {GetEventType(domainEvent)} to Event Grid");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to publish to Event Grid: {ex.Message}");
            throw;
        }
    }

    private async Task PublishToServiceBus<T>(T domainEvent) where T : class
    {
        try
        {
            var sender = _serviceBusClient.CreateSender(_topicName);

            var messageBody = JsonSerializer.Serialize(domainEvent);
            var message = new ServiceBusMessage(messageBody)
            {
                ContentType = "application/json",
                Subject = GetEventType(domainEvent),
                MessageId = GetEventId(domainEvent),
                CorrelationId = GetPatientId(domainEvent).ToString()
            };

            message.ApplicationProperties.Add("EventType", GetEventType(domainEvent));
            message.ApplicationProperties.Add("PatientId", GetPatientId(domainEvent));

            await sender.SendMessageAsync(message);

            _logger.LogInformation($"Published {GetEventType(domainEvent)} to Service Bus");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to publish to Service Bus: {ex.Message}");
            throw;
        }
    }

    private string GetEventType<T>(T domainEvent)
    {
        return domainEvent switch
        {
            PatientCreatedEvent => "PatientCreated",
            PatientUpdatedEvent => "PatientUpdated",
            PatientDeletedEvent => "PatientDeleted",
            _ => "Unknown"
        };
    }

    private int GetPatientId<T>(T domainEvent)
    {
        return domainEvent switch
        {
            PatientCreatedEvent e => e.PatientId,
            PatientUpdatedEvent e => e.PatientId,
            PatientDeletedEvent e => e.PatientId,
            _ => 0
        };
    }

    private string GetEventId<T>(T domainEvent)
    {
        return domainEvent switch
        {
            PatientCreatedEvent e => e.EventId,
            PatientUpdatedEvent e => e.EventId,
            PatientDeletedEvent e => e.EventId,
            _ => Guid.NewGuid().ToString()
        };
    }
}
