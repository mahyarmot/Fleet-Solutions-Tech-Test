using Microsoft.Azure.Cosmos;
using NhsTechTest.Infrastructure.Models;

namespace NhsTechTest.Infrastructure.Seeding;

/// <summary>
/// Seeds initial patient data into CosmosDB
/// </summary>
public static class CosmosDbSeeder
{
    public static async Task SeedDataAsync(Container container)
    {
        // Check if data already exists
        var query = new QueryDefinition("SELECT VALUE COUNT(1) FROM c");
        var iterator = container.GetItemQueryIterator<int>(query);
        var count = 0;

        if (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            count = response.FirstOrDefault();
        }

        if (count > 0)
        {
            Console.WriteLine($"Database already contains {count} patients. Skipping seed.");
            return;
        }

        Console.WriteLine("Seeding initial patient data...");

        var patients = new[]
        {
            new PatientDocument
            {
                Id = Guid.NewGuid().ToString(),
                PatientId = 1,
                NHSNumber = "485 777 3456",
                Name = "Sarah Johnson",
                DateOfBirth = new DateTime(1985, 3, 15),
                GPPractice = "Riverside Medical Centre",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new PatientDocument
            {
                Id = Guid.NewGuid().ToString(),
                PatientId = 2,
                NHSNumber = "943 476 5892",
                Name = "James Anderson",
                DateOfBirth = new DateTime(1978, 7, 22),
                GPPractice = "City Health Clinic",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new PatientDocument
            {
                Id = Guid.NewGuid().ToString(),
                PatientId = 3,
                NHSNumber = "562 839 1047",
                Name = "Emily Williams",
                DateOfBirth = new DateTime(1992, 11, 8),
                GPPractice = "Greenfield Surgery",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new PatientDocument
            {
                Id = Guid.NewGuid().ToString(),
                PatientId = 4,
                NHSNumber = "721 304 8569",
                Name = "Mohammed Ali",
                DateOfBirth = new DateTime(1965, 5, 30),
                GPPractice = "Central Medical Practice",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new PatientDocument
            {
                Id = Guid.NewGuid().ToString(),
                PatientId = 5,
                NHSNumber = "894 652 7103",
                Name = "Catherine Brown",
                DateOfBirth = new DateTime(2001, 9, 17),
                GPPractice = "Riverside Medical Centre",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        foreach (var patient in patients)
        {
            await container.CreateItemAsync(
                patient,
                new PartitionKey(patient.GPPractice));

            Console.WriteLine($"Seeded patient: {patient.Name}");
        }

        Console.WriteLine("Seeding completed successfully!");
    }
}