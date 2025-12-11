using FluentValidation;

namespace Volcanion.LedgerService.Application.Commands.Transactions;

/// <summary>
/// Provides validation rules for the ApplyAdjustmentCommand to ensure that all required fields are present and conform
/// to expected formats and value ranges.
/// </summary>
/// <remarks>This validator enforces constraints such as non-empty fields, maximum string lengths, and positive
/// adjustment amounts. Use this class to validate ApplyAdjustmentCommand instances before processing adjustment
/// operations. Validation failures will include descriptive error messages for each invalid field.</remarks>
public class ApplyAdjustmentCommandValidator : AbstractValidator<ApplyAdjustmentCommand>
{
    /// <summary>
    /// Initializes a new instance of the ApplyAdjustmentCommandValidator class, which enforces validation rules for
    /// adjustment commands.
    /// </summary>
    /// <remarks>This validator ensures that all required fields in an ApplyAdjustmentCommand are present and
    /// conform to expected formats and value ranges. Specifically, it checks that AccountId, TransactionId, Reason, and
    /// AdjustedBy are not empty and do not exceed their respective maximum lengths, and that Amount is a positive
    /// value. Use this validator to verify command data before processing adjustment operations.</remarks>
    public ApplyAdjustmentCommandValidator()
    {
        RuleFor(x => x.AccountId)
            .NotEmpty().WithMessage("AccountId is required");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0 (only positive adjustments allowed)");

        RuleFor(x => x.TransactionId)
            .NotEmpty().WithMessage("TransactionId is required")
            .MaximumLength(100).WithMessage("TransactionId cannot exceed 100 characters");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason is required")
            .MaximumLength(500).WithMessage("Reason cannot exceed 500 characters");

        RuleFor(x => x.AdjustedBy)
            .NotEmpty().WithMessage("AdjustedBy is required")
            .MaximumLength(100).WithMessage("AdjustedBy cannot exceed 100 characters");
    }
}
