using Microsoft.Azure.Cosmos;
using NhsTechTest.Domain.Entities;
using NhsTechTest.Domain.Repositories;
using NhsTechTest.Infrastructure.Configuration;
using NhsTechTest.Infrastructure.Models;

namespace NhsTechTest.Infrastructure.Persistence;

/// <summary>
/// CosmosDB implementation of the Patient repository
/// </summary>
public sealed class CosmosDbPatientRepository : IPatientRepository
{
    private readonly Container _container;
    private int _nextId = 1;

    public CosmosDbPatientRepository(CosmosClient cosmosClient, CosmosDbSettings settings)
    {
        _container = cosmosClient.GetContainer(settings.DatabaseName, settings.ContainerName);
        InitializeNextIdAsync().GetAwaiter().GetResult();
    }

    private async Task InitializeNextIdAsync()
    {
        // Get the highest patient ID to continue sequence
        var query = new QueryDefinition("SELECT VALUE MAX(c.patientId) FROM c");
        var iterator = _container.GetItemQueryIterator<int?>(query);

        if (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            var maxId = response.FirstOrDefault();
            _nextId = (maxId ?? 0) + 1;
        }
    }

    public async Task<Patient?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new QueryDefinition(
                "SELECT * FROM c WHERE c.patientId = @patientId")
                .WithParameter("@patientId", id);

            var iterator = _container.GetItemQueryIterator<PatientDocument>(query);

            if (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync(cancellationToken);
                var document = response.FirstOrDefault();

                return document is not null ? MapToDomain(document) : null;
            }

            return null;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<Patient?> GetByNHSNumberAsync(string nhsNumber, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new QueryDefinition(
                "SELECT * FROM c WHERE c.nhsNumber = @nhsNumber")
                .WithParameter("@nhsNumber", nhsNumber);

            var iterator = _container.GetItemQueryIterator<PatientDocument>(query);

            if (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync(cancellationToken);
                var document = response.FirstOrDefault();

                return document is not null ? MapToDomain(document) : null;
            }

            return null;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<IEnumerable<Patient>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var patients = new List<Patient>();
        var query = new QueryDefinition("SELECT * FROM c");
        var iterator = _container.GetItemQueryIterator<PatientDocument>(query);

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync(cancellationToken);
            patients.AddRange(response.Select(MapToDomain));
        }

        return patients;
    }

    public async Task<int> AddAsync(Patient patient, CancellationToken cancellationToken = default)
    {
        var patientId = _nextId++;
        var document = MapToDocument(patient, patientId);
        document.CreatedAt = DateTime.UtcNow;
        document.UpdatedAt = DateTime.UtcNow;

        await _container.CreateItemAsync(
            document,
            new PartitionKey(document.GPPractice),
            cancellationToken: cancellationToken);

        return patientId;
    }

    public async Task UpdateAsync(Patient patient, CancellationToken cancellationToken = default)
    {
        // First, get the existing document to find its id (GUID) and partition key
        var existingDocument = await GetDocumentByPatientIdAsync(patient.Id, cancellationToken);

        if (existingDocument is null)
        {
            throw new InvalidOperationException($"Patient with ID {patient.Id} not found");
        }

        // Update the document
        var document = MapToDocument(patient, patient.Id);
        document.Id = existingDocument.Id; // Keep the same CosmosDB id
        document.CreatedAt = existingDocument.CreatedAt; // Preserve creation time
        document.UpdatedAt = DateTime.UtcNow;

        await _container.ReplaceItemAsync(
            document,
            document.Id,
            new PartitionKey(document.GPPractice),
            cancellationToken: cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        // First, get the document to find its id (GUID) and partition key
        var document = await GetDocumentByPatientIdAsync(id, cancellationToken);

        if (document is null)
        {
            throw new InvalidOperationException($"Patient with ID {id} not found");
        }

        await _container.DeleteItemAsync<PatientDocument>(
            document.Id,
            new PartitionKey(document.GPPractice),
            cancellationToken: cancellationToken);
    }

    private async Task<PatientDocument?> GetDocumentByPatientIdAsync(int patientId, CancellationToken cancellationToken)
    {
        var query = new QueryDefinition(
            "SELECT * FROM c WHERE c.patientId = @patientId")
            .WithParameter("@patientId", patientId);

        var iterator = _container.GetItemQueryIterator<PatientDocument>(query);

        if (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync(cancellationToken);
            return response.FirstOrDefault();
        }

        return null;
    }

    private static Patient MapToDomain(PatientDocument document)
    {
        return Patient.Create(
            id: document.PatientId,
            nhsNumber: document.NHSNumber,
            name: document.Name,
            dateOfBirth: document.DateOfBirth,
            gpPractice: document.GPPractice);
    }

    private static PatientDocument MapToDocument(Patient patient, int patientId)
    {
        return new PatientDocument
        {
            Id = Guid.NewGuid().ToString(), // CosmosDB requires string id
            PatientId = patientId,
            NHSNumber = patient.NHSNumber,
            Name = patient.Name,
            DateOfBirth = patient.DateOfBirth,
            GPPractice = patient.GPPractice
        };
    }
}