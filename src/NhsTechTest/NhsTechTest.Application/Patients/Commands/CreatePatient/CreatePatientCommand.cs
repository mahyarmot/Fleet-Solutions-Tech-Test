using ErrorOr;
using MediatR;

namespace NhsTechTest.Application.Patients.Commands.CreatePatient;

public record CreatePatientCommand(
    string NHSNumber,
    string Name,
    DateTime DateOfBirth,
    string GPPractice
) : IRequest<ErrorOr<int>>;