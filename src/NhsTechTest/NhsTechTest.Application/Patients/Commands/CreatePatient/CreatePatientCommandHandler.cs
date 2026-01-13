using ErrorOr;
using MediatR;
using NhsTechTest.Domain.Entities;
using NhsTechTest.Domain.Repositories;

namespace NhsTechTest.Application.Patients.Commands.CreatePatient;

public sealed class CreatePatientCommandHandler
    : IRequestHandler<CreatePatientCommand, ErrorOr<int>>
{
    private readonly IPatientRepository _patientRepository;

    public CreatePatientCommandHandler(IPatientRepository patientRepository)
    {
        _patientRepository = patientRepository;
    }

    public async Task<ErrorOr<int>> Handle(
        CreatePatientCommand command,
        CancellationToken cancellationToken)
    {

        if (string.IsNullOrWhiteSpace(command.NHSNumber))
        {
            return Error.Validation(
                code: "InvalidNHSNumber",
                description: "NHS Number cannot be empty.");
        }

        var existingPatient = _patientRepository.GetByNHSNumberAsync(command.NHSNumber, cancellationToken);

        if (existingPatient is not null)
        {
            return Error.Conflict(
                code: "Patient.DuplicateNHSNumber",
                description: $"A patient with NHS Number {command.NHSNumber} already exists");
        }

        var patient = Patient.Create(
             id: 0,
             nhsNumber: command.NHSNumber,
             name: command.Name,
             dateOfBirth: command.DateOfBirth,
             gpPractice: command.GPPractice);

        var newPatientId = await _patientRepository.AddAsync(patient, cancellationToken);

        return newPatientId;
    }
}
