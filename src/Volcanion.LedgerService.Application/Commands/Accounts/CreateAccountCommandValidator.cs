using FluentValidation;

namespace Volcanion.LedgerService.Application.Commands.Accounts;

/// <summary>
/// Provides validation logic for the CreateAccountCommand, ensuring that required fields are present and conform to
/// expected formats.
/// </summary>
/// <remarks>This validator enforces rules for the UserId and Currency properties, including checks for non-empty
/// values, maximum length, and valid ISO 4217 currency codes. Use this class to validate account creation requests
/// before processing them. The validation rules are designed to prevent common input errors and ensure data
/// consistency.</remarks>
public class CreateAccountCommandValidator : AbstractValidator<CreateAccountCommand>
{
    /// <summary>
    /// Initializes a new instance of the CreateAccountCommandValidator class, configuring validation rules for account
    /// creation commands.
    /// </summary>
    /// <remarks>The validator enforces that the UserId is provided and does not exceed 100 characters. It
    /// also ensures that the Currency field is a valid ISO 4217 currency code consisting of exactly 3 characters. Use
    /// this validator to verify the integrity of account creation requests before processing.</remarks>
    public CreateAccountCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required")
            .MaximumLength(100).WithMessage("UserId cannot exceed 100 characters");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required")
            .Length(3).WithMessage("Currency must be 3 characters (ISO 4217)")
            .Must(BeValidCurrency).WithMessage("Invalid currency code");
    }

    /// <summary>
    /// Determines whether the specified currency code is recognized as a valid supported currency.
    /// </summary>
    /// <remarks>Supported currency codes are "USD", "EUR", "GBP", and "JPY". The comparison is
    /// case-insensitive.</remarks>
    /// <param name="currency">The currency code to validate. The value is case-insensitive and must be a non-null, non-empty string
    /// representing a supported currency (e.g., "USD", "EUR", "GBP", "JPY").</param>
    /// <returns>true if the currency code is valid and supported; otherwise, false.</returns>
    private bool BeValidCurrency(string currency)
    {
        var validCurrencies = new[] { "USD", "EUR", "GBP", "JPY", "VND" };
        return validCurrencies.Contains(currency.ToUpperInvariant());
    }
}
