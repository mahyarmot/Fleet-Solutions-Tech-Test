using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NhsTechTest.Application.Patients.Queries;

namespace NhsTechTest.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PatientsController(ISender mediator, ILogger<PatientsController> logger) : ControllerBase
{
    private readonly ISender _mediator = mediator;
    private readonly ILogger<PatientsController> _logger = logger;

    /// <summary>
    /// This endpoint retrieves a patient by their unique identifier.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPatientAsync(
        int id,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving patient with ID: {PatientId}", id);

        var query = new GetPatientQuery(id);
        var result = await _mediator.Send(query, cancellationToken);

        return result.Match(
            patient => Ok(patient),
            errors => Problem(errors));
    }

    private ObjectResult Problem(List<Error> errors)
    {
        if (errors.Count is 0)
        {
            return Problem();
        }

        var firstError = errors[0];

        var statusCode = firstError.Type switch
        {
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };

        _logger.LogWarning(
            "Request failed with error: {ErrorCode} - {ErrorDescription}",
            firstError.Code,
            firstError.Description);

        return Problem(
            statusCode: statusCode,
            title: firstError.Code,
            detail: firstError.Description);
    }
}
