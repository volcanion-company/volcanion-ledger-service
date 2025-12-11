using FluentValidation;

namespace Volcanion.LedgerService.Application.Commands.Transactions;

/// <summary>
/// Provides validation rules for the <see cref="TopupCommand"/> to ensure that required fields are present and values
/// are within acceptable ranges.
/// </summary>
/// <remarks>This validator enforces constraints on the AccountId, Amount, and TransactionId properties of a
/// top-up command. It ensures that AccountId and TransactionId are not empty, TransactionId does not exceed 100
/// characters, and Amount is greater than 0 and does not exceed 1,000,000,000. Use this class to validate top-up
/// requests before processing them.</remarks>
public class TopupCommandValidator : AbstractValidator<TopupCommand>
{
    /// <summary>
    /// Initializes a new instance of the TopupCommandValidator class, which defines validation rules for top-up
    /// commands.
    /// </summary>
    /// <remarks>The validator enforces that AccountId and TransactionId are not empty, TransactionId does not
    /// exceed 100 characters, and Amount is greater than 0 and does not exceed 1,000,000,000. Use this validator to
    /// ensure that top-up command data meets required constraints before processing.</remarks>
    public TopupCommandValidator()
    {
        RuleFor(x => x.AccountId)
            .NotEmpty().WithMessage("AccountId is required");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0")
            .LessThanOrEqualTo(1000000000).WithMessage("Amount cannot exceed 1,000,000,000");

        RuleFor(x => x.TransactionId)
            .NotEmpty().WithMessage("TransactionId is required")
            .MaximumLength(100).WithMessage("TransactionId cannot exceed 100 characters");
    }
}
