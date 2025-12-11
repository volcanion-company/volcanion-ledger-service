using MediatR;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using Volcanion.LedgerService.Application.Commands.Transactions;
using Volcanion.LedgerService.Application.Queries.Transactions;

namespace Volcanion.LedgerService.API.Controllers.V1;

/// <summary>
/// API controller that handles account transaction operations, including top-ups, payments, refunds, adjustments, and
/// retrieval of transaction history.
/// </summary>
/// <remarks>This controller provides endpoints for managing financial transactions on accounts. All endpoints
/// require valid request data and return standard HTTP responses indicating the result of the operation. The controller
/// is versioned and produces JSON responses. Thread safety is ensured by the stateless nature of the controller and the
/// use of dependency-injected services.</remarks>
/// <param name="mediator">The mediator used to send commands and queries related to transaction operations. Must not be null.</param>
/// <param name="logger">The logger used to record diagnostic and operational information for the controller. Must not be null.</param>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public class TransactionsController(IMediator mediator, ILogger<TransactionsController> logger) : ControllerBase
{
    /// <summary>
    /// Processes a top-up request for an account and returns the result of the operation.
    /// </summary>
    /// <param name="request">The top-up request details, including the account identifier, amount, transaction ID, and an optional
    /// description. Cannot be null.</param>
    /// <returns>An <see cref="IActionResult"/> that represents the result of the top-up operation. Returns a 200 OK response
    /// with the result data if successful; otherwise, returns a 400 Bad Request with error details.</returns>
    [HttpPost("topup")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Topup([FromBody] TopupRequest request)
    {
        // Log the incoming request
        logger.LogDebug("Received top-up request: {@Request}", request);
        // Create the command
        var command = new TopupCommand(
            request.AccountId,
            request.Amount,
            request.TransactionId,
            request.Description);

        // Send the command to the mediator
        var result = await mediator.Send(command);
        if (!result.IsSuccess)
        {
            // Return bad request with error details
            return BadRequest(new { error = result.ErrorMessage, errors = result.Errors });
        }
        // Return OK with the result data
        return Ok(result.Data);
    }

    /// <summary>
    /// Processes a payment request and returns the result of the payment operation.
    /// </summary>
    /// <param name="request">The payment details to process. Must not be null and must contain valid account, amount, and transaction
    /// information.</param>
    /// <returns>An <see cref="IActionResult"/> containing the result of the payment operation. Returns a 200 OK response with
    /// the result data if the payment is successful; otherwise, returns a 400 Bad Request response with error details.</returns>
    [HttpPost("payment")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ProcessPayment([FromBody] PaymentRequest request)
    {
        // Log the incoming request
        logger.LogDebug("Received payment request: {@Request}", request);
        // Create the command
        var command = new ProcessPaymentCommand(
            request.AccountId,
            request.Amount,
            request.Fee,
            request.Tax,
            request.TransactionId,
            request.MerchantId,
            request.Description);

        // Send the command to the mediator
        var result = await mediator.Send(command);
        if (!result.IsSuccess)
        {
            // Return bad request with error details
            return BadRequest(new { error = result.ErrorMessage, errors = result.Errors });
        }
        // Return OK with the result data
        return Ok(result.Data);
    }

    /// <summary>
    /// Processes a refund request for a specified transaction.
    /// </summary>
    /// <param name="request">The refund request details, including the account, amount, and transaction information. Cannot be null.</param>
    /// <returns>An <see cref="IActionResult"/> that represents the result of the refund operation. Returns status code 200 (OK)
    /// with the refund result if successful; otherwise, returns status code 400 (Bad Request) with error details.</returns>
    [HttpPost("refund")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ProcessRefund([FromBody] RefundRequest request)
    {
        // Log the incoming request
        logger.LogDebug("Received refund request: {@Request}", request);
        // Create the command
        var command = new ProcessRefundCommand(
            request.AccountId,
            request.Amount,
            request.TransactionId,
            request.OriginalTransactionId,
            request.Description);

        // Send the command to the mediator
        var result = await mediator.Send(command);
        if (!result.IsSuccess)
        {
            // Return bad request with error details
            return BadRequest(new { error = result.ErrorMessage, errors = result.Errors });
        }
        // Return OK with the result data
        return Ok(result.Data);
    }

    /// <summary>
    /// Processes an account adjustment request and applies the specified adjustment to the target account.
    /// </summary>
    /// <param name="request">The adjustment details to apply, including the account identifier, amount, transaction ID, reason, and the user
    /// performing the adjustment. Cannot be null.</param>
    /// <returns>An <see cref="IActionResult"/> that represents the result of the operation. Returns a 200 OK response with the
    /// adjustment result if successful; otherwise, returns a 400 Bad Request with error details.</returns>
    [HttpPost("adjustment")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ApplyAdjustment([FromBody] AdjustmentRequest request)
    {
        // Log the incoming request
        logger.LogDebug("Received adjustment request: {@Request}", request);
        // Create the command
        var command = new ApplyAdjustmentCommand(
            request.AccountId,
            request.Amount,
            request.TransactionId,
            request.Reason,
            request.AdjustedBy);

        // Send the command to the mediator
        var result = await mediator.Send(command);
        if (!result.IsSuccess)
        {
            // Return bad request with error details
            return BadRequest(new { error = result.ErrorMessage, errors = result.Errors });
        }
        // Return OK with the result data
        return Ok(result.Data);
    }

    /// <summary>
    /// Retrieves a paginated list of transaction history records for the specified account.
    /// </summary>
    /// <param name="accountId">The unique identifier of the account for which to retrieve transaction history.</param>
    /// <param name="page">The page number of results to return. Must be greater than or equal to 1. Defaults to 1.</param>
    /// <param name="pageSize">The maximum number of records to return per page. Must be greater than 0. Defaults to 50.</param>
    /// <param name="transactionType">An optional filter specifying the type of transactions to include. If null, all transaction types are included.</param>
    /// <param name="startDate">An optional filter specifying the earliest transaction date to include. Transactions occurring before this date
    /// are excluded.</param>
    /// <param name="endDate">An optional filter specifying the latest transaction date to include. Transactions occurring after this date are
    /// excluded.</param>
    /// <returns>An IActionResult containing a paginated list of transaction history records if the request is successful;
    /// otherwise, a BadRequest result with error details.</returns>
    [HttpGet("history/{accountId:guid}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetTransactionHistory(
        Guid accountId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? transactionType = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        // Log the incoming request
        logger.LogDebug("Fetching transaction history for AccountId: {AccountId}, Page: {Page}, PageSize: {PageSize}, TransactionType: {TransactionType}, StartDate: {StartDate}, EndDate: {EndDate}",
            accountId, page, pageSize, transactionType, startDate, endDate);
        // Create the query
        var query = new GetTransactionHistoryQuery(
            accountId,
            page,
            pageSize,
            transactionType,
            startDate,
            endDate);

        // Send the query to the mediator
        var result = await mediator.Send(query);
        if (!result.IsSuccess)
        {
            // Return bad request with error details
            return BadRequest(new { error = result.ErrorMessage, errors = result.Errors });
        }
        // Return OK with the result data
        return Ok(result.Data);
    }
}

/// <summary>
/// Represents a request to add funds to an account with the specified amount and transaction details.
/// </summary>
/// <param name="AccountId">The unique identifier of the account to which the funds will be credited.</param>
/// <param name="Amount">The amount to be added to the account balance. Must be a positive value.</param>
/// <param name="TransactionId">A unique identifier for the top-up transaction. Used to ensure idempotency and track the operation.</param>
/// <param name="Description">An optional description providing additional context or information about the top-up transaction.</param>
public record TopupRequest(
    Guid AccountId,
    decimal Amount,
    string TransactionId,
    string? Description);

/// <summary>
/// Represents a request to process a payment, including account, amount, and transaction details.
/// </summary>
/// <param name="AccountId">The unique identifier of the account from which the payment will be made.</param>
/// <param name="Amount">The total amount to be charged for the payment, excluding fees and taxes. Must be a non-negative value.</param>
/// <param name="Fee">The fee amount to be applied to the payment. Must be a non-negative value.</param>
/// <param name="Tax">The tax amount to be applied to the payment. Must be a non-negative value.</param>
/// <param name="TransactionId">The unique identifier for the payment transaction. Cannot be null or empty.</param>
/// <param name="MerchantId">The unique identifier of the merchant receiving the payment. Cannot be null or empty.</param>
/// <param name="Description">An optional description or note associated with the payment. Can be null.</param>
public record PaymentRequest(
    Guid AccountId,
    decimal Amount,
    decimal Fee,
    decimal Tax,
    string TransactionId,
    string MerchantId,
    string? Description);

/// <summary>
/// Represents a request to initiate a refund for a specific transaction on an account.
/// </summary>
/// <param name="AccountId">The unique identifier of the account to which the refund will be applied.</param>
/// <param name="Amount">The amount to refund. Must be a positive value representing the currency to be refunded.</param>
/// <param name="TransactionId">The unique identifier of the transaction associated with this refund request.</param>
/// <param name="OriginalTransactionId">The unique identifier of the original transaction that is being refunded.</param>
/// <param name="Description">An optional description providing additional context or reason for the refund. Can be null.</param>
public record RefundRequest(
    Guid AccountId,
    decimal Amount,
    string TransactionId,
    string OriginalTransactionId,
    string? Description);

/// <summary>
/// Represents a request to adjust the balance of an account, including details about the adjustment and its origin.
/// </summary>
/// <param name="AccountId">The unique identifier of the account for which the adjustment is requested.</param>
/// <param name="Amount">The amount to adjust the account balance by. Positive values increase the balance; negative values decrease it.</param>
/// <param name="TransactionId">The unique identifier of the transaction associated with this adjustment. Used for tracking and idempotency.</param>
/// <param name="Reason">A description of the reason for the adjustment. Provides context for auditing and review.</param>
/// <param name="AdjustedBy">The identifier of the user or system that initiated the adjustment.</param>
public record AdjustmentRequest(
    Guid AccountId,
    decimal Amount,
    string TransactionId,
    string Reason,
    string AdjustedBy);
