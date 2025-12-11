using MediatR;
using Volcanion.LedgerService.Application.Common;
using Volcanion.LedgerService.Application.DTOs;

namespace Volcanion.LedgerService.Application.Commands.Transactions;

/// <summary>
/// Represents a command to apply an adjustment to an account ledger transaction.
/// </summary>
/// <param name="AccountId">The unique identifier of the account to which the adjustment will be applied.</param>
/// <param name="Amount">The monetary amount of the adjustment to be applied. Positive values increase the account balance; negative values
/// decrease it.</param>
/// <param name="TransactionId">The unique identifier for the transaction associated with this adjustment.</param>
/// <param name="Reason">A description of the reason for the adjustment. This is used for audit and tracking purposes.</param>
/// <param name="AdjustedBy">The identifier of the user or system that is performing the adjustment.</param>
public record ApplyAdjustmentCommand(
    Guid AccountId,
    decimal Amount,
    string TransactionId,
    string Reason,
    string AdjustedBy) : IRequest<Result<LedgerTransactionDto>>;
