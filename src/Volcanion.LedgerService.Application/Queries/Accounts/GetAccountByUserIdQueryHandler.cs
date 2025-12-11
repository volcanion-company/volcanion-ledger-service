using MediatR;
using Microsoft.Extensions.Logging;
using Volcanion.LedgerService.Application.Common;
using Volcanion.LedgerService.Application.DTOs;
using Volcanion.LedgerService.Domain.Repositories;

namespace Volcanion.LedgerService.Application.Queries.Accounts;

public class GetAccountByUserIdQueryHandler : IRequestHandler<GetAccountByUserIdQuery, Result<AccountDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetAccountByUserIdQueryHandler> _logger;

    public GetAccountByUserIdQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetAccountByUserIdQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<AccountDto>> Handle(GetAccountByUserIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var account = await _unitOfWork.Accounts.GetByUserIdAsync(request.UserId, cancellationToken);
            
            if (account == null)
            {
                _logger.LogWarning("Account for user {UserId} not found", request.UserId);
                return Result<AccountDto>.Failure($"Account for user {request.UserId} not found");
            }

            var dto = new AccountDto
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

            return Result<AccountDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving account for user {UserId}", request.UserId);
            return Result<AccountDto>.Failure($"Error retrieving account: {ex.Message}");
        }
    }
}
