using FluentValidation;

namespace Volcanion.LedgerService.Application.Commands.Transactions;

/// <summary>
/// Provides validation rules for the <see cref="ProcessRefundCommand"/> to ensure that all required fields are present
/// and meet expected constraints before processing a refund request.
/// </summary>
/// <remarks>This validator enforces that the account ID, transaction ID, and original transaction ID are not
/// empty and that the transaction IDs do not exceed 100 characters. It also ensures that the refund amount is greater
/// than zero. Use this class to validate refund commands prior to executing business logic to prevent invalid or
/// incomplete data from being processed.</remarks>
public class ProcessRefundCommandValidator : AbstractValidator<ProcessRefundCommand>
{
    /// <summary>
    /// Initializes a new instance of the ProcessRefundCommandValidator class, which enforces validation rules for
    /// processing refund commands.
    /// </summary>
    /// <remarks>The validator ensures that required fields such as AccountId, TransactionId, and
    /// OriginalTransactionId are not empty and that TransactionId and OriginalTransactionId do not exceed 100
    /// characters. It also enforces that the Amount is greater than zero. These rules help prevent invalid refund
    /// requests from being processed.</remarks>
    public ProcessRefundCommandValidator()
    {
        RuleFor(x => x.AccountId)
            .NotEmpty().WithMessage("AccountId is required");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0");

        RuleFor(x => x.TransactionId)
            .NotEmpty().WithMessage("TransactionId is required")
            .MaximumLength(100).WithMessage("TransactionId cannot exceed 100 characters");

        RuleFor(x => x.OriginalTransactionId)
            .NotEmpty().WithMessage("OriginalTransactionId is required")
            .MaximumLength(100).WithMessage("OriginalTransactionId cannot exceed 100 characters");
    }
}
