using MediatR;
using Microsoft.Extensions.Logging;
using Volcanion.LedgerService.Application.Common;
using Volcanion.LedgerService.Application.DTOs;
using Volcanion.LedgerService.Domain.Entities;
using Volcanion.LedgerService.Domain.Repositories;

namespace Volcanion.LedgerService.Application.Commands.Accounts;

/// <summary>
/// Handles the creation of new user accounts in response to a create account command.
/// </summary>
/// <remarks>This handler checks for existing accounts before creating a new one and logs relevant events. It
/// returns a result indicating success or failure, including error details if account creation fails.</remarks>
/// <param name="unitOfWork">The unit of work instance used to access account data and persist changes.</param>
/// <param name="logger">The logger used to record informational and error messages during account creation.</param>
public class CreateAccountCommandHandler(
    IUnitOfWork unitOfWork,
    ILogger<CreateAccountCommandHandler> logger) : IRequestHandler<CreateAccountCommand, Result<AccountDto>>
{
    /// <summary>
    /// Handles the creation of a new account for the specified user and currency.
    /// </summary>
    /// <remarks>If an account already exists for the specified user, the operation fails and no new account
    /// is created. The method logs relevant information and errors during the process.</remarks>
    /// <param name="request">The command containing the user identifier and currency information for the account to be created.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the account creation operation.</param>
    /// <returns>A result containing the created account data if successful; otherwise, a failure result with an error message.</returns>
    public async Task<Result<AccountDto>> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Log the account creation attempt
            logger.LogInformation("Creating account for user {UserId} with currency {Currency}", request.UserId, request.Currency);

            // Check if account already exists for this user
            var existingAccount = await unitOfWork.Accounts.GetByUserIdAsync(request.UserId, cancellationToken);
            if (existingAccount != null)
            {
                logger.LogWarning("Account already exists for user {UserId}", request.UserId);
                return Result<AccountDto>.Failure($"Account already exists for user {request.UserId}");
            }

            // Create new account
            var account = Account.Create(request.UserId, request.Currency);

            await unitOfWork.Accounts.AddAsync(account, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            // Log successful account creation
            logger.LogInformation("Account {AccountId} created successfully for user {UserId}", account.Id, request.UserId);
            // Map to DTO and return success result
            var dto = MapToDto(account);
            return Result<AccountDto>.Success(dto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating account for user {UserId}", request.UserId);
            return Result<AccountDto>.Failure($"Error creating account: {ex.Message}");
        }
    }

    /// <summary>
    /// Maps the specified <see cref="Account"/> entity to an <see cref="AccountDto"/> data transfer object.
    /// </summary>
    /// <param name="account">The account entity to be mapped. Cannot be null.</param>
    /// <returns>An <see cref="AccountDto"/> instance containing the mapped data from the specified account.</returns>
    private static AccountDto MapToDto(Account account)
    {
        return new AccountDto
        {
            Id = account.Id,
            AccountNumber = account.AccountNumber,
            UserId = account.UserId,
            Balance = account.Balance.Amount,
            AvailableBalance = account.AvailableBalance.Amount,
            ReservedBalance = account.ReservedBalance.Amount,
            Currency = account.Currency,
            IsActive = account.IsActive,
            LockedAt = account.LockedAt,
            LockedReason = account.LockedReason,
            CreatedAt = account.CreatedAt,
            UpdatedAt = account.UpdatedAt
        };
    }
}
