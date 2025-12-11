using MediatR;
using Microsoft.Extensions.Logging;
using Volcanion.LedgerService.Application.Common;
using Volcanion.LedgerService.Application.DTOs;
using Volcanion.LedgerService.Domain.Repositories;

namespace Volcanion.LedgerService.Application.Queries.Accounts;

public class GetAccountByIdQueryHandler : IRequestHandler<GetAccountByIdQuery, Result<AccountDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetAccountByIdQueryHandler> _logger;

    public GetAccountByIdQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetAccountByIdQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<AccountDto>> Handle(GetAccountByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var account = await _unitOfWork.Accounts.GetByIdAsync(request.AccountId, cancellationToken);
            
            if (account == null)
            {
                _logger.LogWarning("Account {AccountId} not found", request.AccountId);
                return Result<AccountDto>.Failure($"Account {request.AccountId} not found");
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
            _logger.LogError(ex, "Error retrieving account {AccountId}", request.AccountId);
            return Result<AccountDto>.Failure($"Error retrieving account: {ex.Message}");
        }
    }
}
