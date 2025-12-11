using FluentValidation;

namespace Volcanion.LedgerService.Application.Commands.Transactions;

/// <summary>
/// Provides validation rules for the ProcessPaymentCommand to ensure that payment details meet required criteria before
/// processing.
/// </summary>
/// <remarks>This validator enforces constraints such as non-empty account and transaction identifiers, positive
/// payment amounts, and maximum length restrictions for certain fields. Use this class to validate payment commands
/// prior to executing payment operations, helping to prevent invalid or incomplete data from being processed.</remarks>
public class ProcessPaymentCommandValidator : AbstractValidator<ProcessPaymentCommand>
{
    /// <summary>
    /// Initializes a new instance of the ProcessPaymentCommandValidator class, configuring validation rules for payment
    /// processing commands.
    /// </summary>
    /// <remarks>The validator enforces required fields and value constraints for properties such as
    /// AccountId, Amount, Fee, Tax, TransactionId, and MerchantId. Use this validator to ensure that payment command
    /// data meets the expected format and business rules before processing.</remarks>
    public ProcessPaymentCommandValidator()
    {
        RuleFor(x => x.AccountId)
            .NotEmpty().WithMessage("AccountId is required");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0")
            .LessThanOrEqualTo(1000000000).WithMessage("Amount cannot exceed 1,000,000,000");

        RuleFor(x => x.Fee)
            .GreaterThanOrEqualTo(0).WithMessage("Fee must be greater than or equal to 0");

        RuleFor(x => x.Tax)
            .GreaterThanOrEqualTo(0).WithMessage("Tax must be greater than or equal to 0");

        RuleFor(x => x.TransactionId)
            .NotEmpty().WithMessage("TransactionId is required")
            .MaximumLength(100).WithMessage("TransactionId cannot exceed 100 characters");

        RuleFor(x => x.MerchantId)
            .NotEmpty().WithMessage("MerchantId is required")
            .MaximumLength(100).WithMessage("MerchantId cannot exceed 100 characters");
    }
}
