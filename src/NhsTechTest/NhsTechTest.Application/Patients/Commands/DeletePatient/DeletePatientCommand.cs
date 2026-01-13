using ErrorOr;
using MediatR;

namespace NhsTechTest.Application.Patients.Commands.DeletePatient;

public record DeletePatientCommand(int Id) : IRequest<ErrorOr<Success>>;