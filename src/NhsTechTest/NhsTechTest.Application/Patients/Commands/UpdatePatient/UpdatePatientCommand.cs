using ErrorOr;
using MediatR;

namespace NhsTechTest.Application.Patients.Commands.UpdatePatient;

public record UpdatePatientCommand(
    int Id,
    string NHSNumber,
    string Name,
    DateTime DateOfBirth,
    string GPPractice
) : IRequest<ErrorOr<Success>>;
