using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NhsTechTest.Application.Patients.Commands.CreatePatient;
using NhsTechTest.Application.Patients.Commands.DeletePatient;
using NhsTechTest.Application.Patients.Commands.UpdatePatient;
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
    public async Task<IActionResult> GetPatient(
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


    /// <summary>
    /// Creates a new patient
    /// </summary>
    /// <param name="command">Patient creation details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The ID of the newly created patient</returns>
    /// <response code="201">Patient created successfully</response>
    /// <response code="400">Invalid input data</response>
    /// <response code="409">Patient with same NHS Number already exists</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreatePatient(
        [FromBody] CreatePatientCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating new patient with NHS Number: {NHSNumber}", command.NHSNumber);

        var result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            patientId => CreatedAtAction(
                nameof(GetPatient),
                new { id = patientId },
                new { id = patientId }),
            errors => Problem(errors));
    }

    /// <summary>
    /// Updates an existing patient
    /// </summary>
    /// <param name="id">The patient's unique identifier</param>
    /// <param name="command">Updated patient details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content on success</returns>
    /// <response code="204">Patient updated successfully</response>
    /// <response code="400">Invalid input data</response>
    /// <response code="404">Patient not found</response>
    /// <response code="409">NHS Number already assigned to another patient</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdatePatient(
        int id,
        [FromBody] UpdatePatientCommand command,
        CancellationToken cancellationToken)
    {
        // Ensure route ID matches command ID
        if (id != command.Id)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Patient.IdMismatch",
                Detail = "The ID in the URL does not match the ID in the request body"
            });
        }

        _logger.LogInformation("Updating patient with ID: {PatientId}", id);

        var result = await _mediator.Send(command, cancellationToken);

        return result.Match<IActionResult>(
            _ => NoContent(),
            errors => Problem(errors));
    }

    /// <summary>
    /// Deletes a patient
    /// </summary>
    /// <param name="id">The patient's unique identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content on success</returns>
    /// <response code="204">Patient deleted successfully</response>
    /// <response code="400">Invalid patient ID</response>
    /// <response code="404">Patient not found</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePatient(
        int id,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting patient with ID: {PatientId}", id);

        var command = new DeletePatientCommand(id);
        var result = await _mediator.Send(command, cancellationToken);

        return result.Match<IActionResult>(
            _ => NoContent(),
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
