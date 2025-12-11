using MediatR;
using Volcanion.LedgerService.Application.Common;
using Volcanion.LedgerService.Application.DTOs;

namespace Volcanion.LedgerService.Application.Commands.Transactions;

/// <summary>
/// Represents a command to process a refund for a specific account and transaction.
/// </summary>
/// <param name="AccountId">The unique identifier of the account to which the refund will be applied.</param>
/// <param name="Amount">The amount to refund. Must be a positive value representing the currency to be credited.</param>
/// <param name="TransactionId">The unique identifier for the refund transaction. Used to track and reference the refund operation.</param>
/// <param name="OriginalTransactionId">The unique identifier of the original transaction that is being refunded.</param>
/// <param name="Description">An optional description providing additional context or notes about the refund.</param>
public record ProcessRefundCommand(
    Guid AccountId,
    decimal Amount,
    string TransactionId,
    string OriginalTransactionId,
    string? Description = null) : IRequest<Result<LedgerTransactionDto>>;
