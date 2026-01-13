using System.Text.Json.Serialization;

namespace NhsTechTest.Infrastructure.Models;

/// <summary>
/// Patient document model for CosmosDB storage
/// Maps domain entity to document database format
/// </summary>
public class PatientDocument
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("patientId")]
    public int PatientId { get; set; }

    [JsonPropertyName("nhsNumber")]
    public string NHSNumber { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("dateOfBirth")]
    public DateTime DateOfBirth { get; set; }

    [JsonPropertyName("gpPractice")]
    public string GPPractice { get; set; } = string.Empty;

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; }

    [JsonPropertyName("_etag")]
    public string? ETag { get; set; }
}