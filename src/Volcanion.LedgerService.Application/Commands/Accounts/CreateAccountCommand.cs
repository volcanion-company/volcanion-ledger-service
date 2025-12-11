using MediatR;
using Volcanion.LedgerService.Application.Common;
using Volcanion.LedgerService.Application.DTOs;

namespace Volcanion.LedgerService.Application.Commands.Accounts;

/// <summary>
/// Represents a command to create a new account for a specified user with the given currency.
/// </summary>
/// <param name="UserId">The unique identifier of the user for whom the account will be created. Cannot be null or empty.</param>
/// <param name="Currency">The currency code to associate with the new account. Defaults to "VND" if not specified.</param>
public record CreateAccountCommand(string UserId, string Currency = "VND") : IRequest<Result<AccountDto>>;
