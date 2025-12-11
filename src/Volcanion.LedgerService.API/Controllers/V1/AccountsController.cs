using MediatR;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using Volcanion.LedgerService.Application.Commands.Accounts;
using Volcanion.LedgerService.Application.Queries.Accounts;

namespace Volcanion.LedgerService.API.Controllers.V1;

/// <summary>
/// Defines API endpoints for managing user accounts, including creating new accounts and retrieving account information
/// by account ID or user ID.
/// </summary>
/// <remarks>This controller is versioned at 1.0 and produces JSON responses. All endpoints require valid input
/// parameters and return appropriate HTTP status codes based on the outcome of the operation.</remarks>
/// <param name="mediator">The mediator used to send commands and queries for account operations. Must not be null.</param>
/// <param name="logger">The logger used to record diagnostic and operational information for the controller. Must not be null.</param>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/accounts")]
[Produces("application/json")]
public class AccountsController(IMediator mediator, ILogger<AccountsController> logger) : ControllerBase
{
    /// <summary>
    /// Creates a new account using the specified account creation details.
    /// </summary>
    /// <remarks>If the account creation fails due to validation errors or other issues, the response includes
    /// error information in the response body. The location header of the 201 Created response points to the newly
    /// created account resource.</remarks>
    /// <param name="request">The account creation information provided in the request body. Must include a valid user identifier and
    /// currency.</param>
    /// <returns>A 201 Created response containing the newly created account if the operation succeeds; otherwise, a 400 Bad
    /// Request response with error details.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAccount([FromBody] CreateAccountRequest request)
    {
        // Log the account creation attempt
        logger.LogDebug("Creating account for user {UserId} with currency {Currency}", request.UserId, request.Currency);
        // Create the command to create a new account
        var command = new CreateAccountCommand(request.UserId, request.Currency);

        // Send the command to the mediator and await the result
        var result = await mediator.Send(command);
        if (!result.IsSuccess)
        {
            // Return a bad request response with error details if account creation fails
            return BadRequest(new { error = result.ErrorMessage, errors = result.Errors });
        }
        // Return a created response with the location of the new account
        return CreatedAtAction(
            nameof(GetAccountById),
            new { id = result.Data!.Id },
            result.Data);
    }

    /// <summary>
    /// Retrieves the account details for the specified account identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the account to retrieve.</param>
    /// <returns>An <see cref="IActionResult"/> containing the account details with status code 200 if found; otherwise, a 404
    /// Not Found result if the account does not exist.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAccountById(Guid id)
    {
        // Log the account retrieval attempt
        logger.LogDebug("Retrieving account with ID {AccountId}", id);
        // Create the query to get the account by ID
        var query = new GetAccountByIdQuery(id);

        // Send the query to the mediator and await the result
        var result = await mediator.Send(query);
        if (!result.IsSuccess)
        {
            // Return a not found response if the account does not exist
            return NotFound(new { error = result.ErrorMessage });
        }
        // Return the account details with an OK response
        return Ok(result.Data);
    }

    /// <summary>
    /// Retrieves the account information associated with the specified user identifier.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose account information is to be retrieved. Cannot be null or empty.</param>
    /// <returns>An <see cref="IActionResult"/> containing the account information if found; otherwise, a 404 Not Found response
    /// if no account exists for the specified user.</returns>
    [HttpGet("user/{userId}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAccountByUserId(string userId)
    {
        // Log the account retrieval attempt by user ID
        logger.LogDebug("Retrieving account for user {UserId}", userId);
        // Create the query to get the account by user ID
        var query = new GetAccountByUserIdQuery(userId);

        // Send the query to the mediator and await the result
        var result = await mediator.Send(query);
        if (!result.IsSuccess)
        {
            // Return a not found response if no account exists for the user
            return NotFound(new { error = result.ErrorMessage });
        }
        // Return the account details with an OK response
        return Ok(result.Data);
    }
}

/// <summary>
/// Represents a request to create a new account with a specified user identifier and currency.
/// </summary>
/// <param name="UserId">The unique identifier of the user for whom the account is being created. Cannot be null or empty.</param>
/// <param name="Currency">The ISO 4217 currency code to use for the account. Defaults to "VND" if not specified.</param>
public record CreateAccountRequest(string UserId, string Currency = "VND");
