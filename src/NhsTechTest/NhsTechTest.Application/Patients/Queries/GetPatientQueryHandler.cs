using ErrorOr;
using MediatR;
using NhsTechTest.Application.Patients.DTOs;
using NhsTechTest.Domain.Repositories;

namespace NhsTechTest.Application.Patients.Queries;

public sealed class GetPatientQueryHandler
    : IRequestHandler<GetPatientQuery, ErrorOr<PatientSummaryDto>>
{
    private readonly IPatientRepository _patientRepository;

    public GetPatientQueryHandler(IPatientRepository patientRepository)
    {
        _patientRepository = patientRepository;
    }

    public async Task<ErrorOr<PatientSummaryDto>> Handle(
        GetPatientQuery query,
        CancellationToken cancellationToken)
    {
        if (query.PatientId <= 0)
        {
            return Error.Validation(
                code: "Patient.InvalidId",
                description: "Patient ID must be a positive number");
        }

        var patient = await _patientRepository.GetByIdAsync(
            query.PatientId,
            cancellationToken);

        if (patient is null)
        {
            return Error.NotFound(
                code: "Patient.NotFound",
                description: $"No patient found with ID {query.PatientId}");
        }

        return new PatientSummaryDto(
            patient.Id,
            patient.NHSNumber,
            patient.Name,
            patient.DateOfBirth,
            patient.GPPractice);
    }
}
