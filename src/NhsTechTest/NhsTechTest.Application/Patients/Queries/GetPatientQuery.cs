using ErrorOr;
using MediatR;
using NhsTechTest.Application.Patients.DTOs;

namespace NhsTechTest.Application.Patients.Queries;

/// <summary>
/// Query to retrieve a patient summary by ID
/// </summary>
/// <param name="PatientId">The unique identifier of the patient</param>
public record GetPatientQuery(int PatientId) : IRequest<ErrorOr<PatientSummaryDto>>;
