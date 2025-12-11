using MediatR;
using Volcanion.LedgerService.Application.Common;
using Volcanion.LedgerService.Application.DTOs;

namespace Volcanion.LedgerService.Application.Commands.Transactions;

/// <summary>
/// Represents a command to process a payment transaction for a specified account, including amount, fees, taxes, and
/// related metadata.
/// </summary>
/// <remarks>This command is typically used in payment processing workflows to initiate ledger updates and record
/// transaction details. Ensure that all monetary values are validated before creating the command.</remarks>
/// <param name="AccountId">The unique identifier of the account to which the payment will be applied.</param>
/// <param name="Amount">The total amount to be processed for the payment. Must be a positive value.</param>
/// <param name="Fee">The fee amount to be applied to the transaction. Must be zero or positive.</param>
/// <param name="Tax">The tax amount associated with the transaction. Must be zero or positive.</param>
/// <param name="TransactionId">The unique identifier for the payment transaction. Used for tracking and idempotency.</param>
/// <param name="MerchantId">The identifier of the merchant associated with the transaction.</param>
/// <param name="Description">An optional description or note for the transaction. Can be null if no description is provided.</param>
public record ProcessPaymentCommand(
    Guid AccountId,
    decimal Amount,
    decimal Fee,
    decimal Tax,
    string TransactionId,
    string MerchantId,
    string? Description = null) : IRequest<Result<LedgerTransactionDto>>;
