using MediatR;
using Volcanion.LedgerService.Application.Common;
using Volcanion.LedgerService.Application.DTOs;

namespace Volcanion.LedgerService.Application.Commands.Transactions;

/// <summary>
/// Represents a request to add funds to an account as a top-up transaction.
/// </summary>
/// <remarks>This command is typically used to initiate a ledger transaction that increases the account balance.
/// The transaction ID should be unique to prevent duplicate processing. The amount must be greater than zero; negative
/// or zero values are not permitted.</remarks>
/// <param name="AccountId">The unique identifier of the account to which the funds will be credited.</param>
/// <param name="Amount">The amount to be credited to the account. Must be a positive value.</param>
/// <param name="TransactionId">The unique identifier for the top-up transaction. Used for idempotency and tracking.</param>
/// <param name="Description">An optional description for the top-up transaction. Can be null if no description is provided.</param>
public record TopupCommand(
    Guid AccountId,
    decimal Amount,
    string TransactionId,
    string? Description = null) : IRequest<Result<LedgerTransactionDto>>;
