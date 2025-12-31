namespace NhsTechTest.Application.Patients.DTOs;

public record PatientSummaryDto(
    int Id,
    string NHSNumber,
    string Name,
    DateTime DateOfBirth,
    string GPPractice);
