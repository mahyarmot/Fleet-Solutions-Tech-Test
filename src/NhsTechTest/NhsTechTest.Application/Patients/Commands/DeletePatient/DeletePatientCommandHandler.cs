using ErrorOr;
using MediatR;
using NhsTechTest.Domain.Repositories;

namespace NhsTechTest.Application.Patients.Commands.DeletePatient;

public sealed class DeletePatientCommandHandler : IRequestHandler<DeletePatientCommand, ErrorOr<Success>>
{
    private readonly IPatientRepository _patientRepository;

    public DeletePatientCommandHandler(IPatientRepository patientRepository)
    {
        _patientRepository = patientRepository;
    }

    public async Task<ErrorOr<Success>> Handle(
        DeletePatientCommand command,
        CancellationToken cancellationToken)
    {
        // Validate input
        if (command.Id <= 0)
        {
            return Error.Validation(
                code: "Patient.InvalidId",
                description: "Patient ID must be a positive number");
        }

        // Check if patient exists
        var existingPatient = await _patientRepository.GetByIdAsync(
            command.Id,
            cancellationToken);

        if (existingPatient is null)
        {
            return Error.NotFound(
                code: "Patient.NotFound",
                description: $"Patient with ID {command.Id} was not found");
        }

        // Delete from repository
        await _patientRepository.DeleteAsync(command.Id, cancellationToken);

        return Result.Success;
    }
}
