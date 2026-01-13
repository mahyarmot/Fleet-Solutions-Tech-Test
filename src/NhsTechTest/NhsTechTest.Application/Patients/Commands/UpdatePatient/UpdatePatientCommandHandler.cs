using ErrorOr;
using MediatR;
using NhsTechTest.Domain.Entities;
using NhsTechTest.Domain.Repositories;

namespace NhsTechTest.Application.Patients.Commands.UpdatePatient;

public sealed class UpdatePatientCommandHandler
    : IRequestHandler<UpdatePatientCommand, ErrorOr<Success>>
{
    private readonly IPatientRepository _patientRepository;

    public UpdatePatientCommandHandler(IPatientRepository patientRepository)
    {
        _patientRepository = patientRepository;
    }

    public async Task<ErrorOr<Success>> Handle(
        UpdatePatientCommand command,
        CancellationToken cancellationToken)
    {
        // Validate input
        if (command.Id <= 0)
        {
            return Error.Validation(
                code: "Patient.InvalidId",
                description: "Patient ID must be a positive number");
        }

        if (string.IsNullOrWhiteSpace(command.NHSNumber))
        {
            return Error.Validation(
                code: "Patient.InvalidNHSNumber",
                description: "NHS Number is required");
        }

        if (string.IsNullOrWhiteSpace(command.Name))
        {
            return Error.Validation(
                code: "Patient.InvalidName",
                description: "Patient name is required");
        }

        if (command.DateOfBirth > DateTime.UtcNow)
        {
            return Error.Validation(
                code: "Patient.InvalidDateOfBirth",
                description: "Date of birth cannot be in the future");
        }

        if (string.IsNullOrWhiteSpace(command.GPPractice))
        {
            return Error.Validation(
                code: "Patient.InvalidGPPractice",
                description: "GP Practice is required");
        }

        var existingPatient = await _patientRepository.GetByIdAsync(
            command.Id,
            cancellationToken);

        if (existingPatient is null)
        {
            return Error.NotFound(
                code: "Patient.NotFound",
                description: $"Patient with ID {command.Id} was not found");
        }

        var patientWithSameNHS = _patientRepository.GetByNHSNumberAsync(
            command.NHSNumber,
            cancellationToken);

        if (patientWithSameNHS is not null && patientWithSameNHS.Id != command.Id)
        {
            return Error.Conflict(
                code: "Patient.DuplicateNHSNumber",
                description: $"NHS Number {command.NHSNumber} is already assigned to another patient");
        }

        var updatedPatient = Patient.Create(
            id: command.Id,
            nhsNumber: command.NHSNumber,
            name: command.Name,
            dateOfBirth: command.DateOfBirth,
            gpPractice: command.GPPractice);

        await _patientRepository.UpdateAsync(updatedPatient, cancellationToken);

        return Result.Success;
    }
}