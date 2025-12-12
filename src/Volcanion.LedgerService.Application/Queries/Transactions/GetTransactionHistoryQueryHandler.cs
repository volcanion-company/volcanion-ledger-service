using MediatR;
using Microsoft.Extensions.Logging;
using Volcanion.LedgerService.Application.Common;
using Volcanion.LedgerService.Application.DTOs;
using Volcanion.LedgerService.Domain.Entities;
using Volcanion.LedgerService.Domain.Repositories;
using Volcanion.LedgerService.Domain.ValueObjects;

namespace Volcanion.LedgerService.Application.Queries.Transactions;

public class GetTransactionHistoryQueryHandler 
    : IRequestHandler<GetTransactionHistoryQuery, Result<PagedResult<LedgerTransactionDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetTransactionHistoryQueryHandler> _logger;

    public GetTransactionHistoryQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetTransactionHistoryQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<PagedResult<LedgerTransactionDto>>> Handle(
        GetTransactionHistoryQuery request, 
        CancellationToken cancellationToken)
    {
        try
        {
            List<LedgerTransactionDto> transactions;
            int totalCount;

            // Filter by date range if provided
            if (request.StartDate.HasValue && request.EndDate.HasValue)
            {
                totalCount = await _unitOfWork.LedgerTransactions.GetCountByDateRangeAsync(
                    request.AccountId,
                    request.StartDate.Value,
                    request.EndDate.Value,
                    cancellationToken);

                var pagedTransactions = await _unitOfWork.LedgerTransactions.GetByDateRangeAsync(
                    request.AccountId,
                    request.StartDate.Value,
                    request.EndDate.Value,
                    request.Page,
                    request.PageSize,
                    cancellationToken);

                transactions = pagedTransactions
                    .Select(MapToDto)
                    .ToList();
            }
            // Filter by transaction type if provided
            else if (!string.IsNullOrEmpty(request.TransactionType))
            {
                var type = TransactionType.FromString(request.TransactionType);
                
                totalCount = await _unitOfWork.LedgerTransactions.GetCountByAccountIdAndTypeAsync(
                    request.AccountId,
                    type,
                    cancellationToken);

                var pagedTransactions = await _unitOfWork.LedgerTransactions.GetByAccountIdAndTypeAsync(
                    request.AccountId,
                    type,
                    request.Page,
                    request.PageSize,
                    cancellationToken);

                transactions = pagedTransactions
                    .Select(MapToDto)
                    .ToList();
            }
            // Get all transactions with pagination
            else
            {
                totalCount = await _unitOfWork.LedgerTransactions.GetCountByAccountIdAsync(
                    request.AccountId,
                    cancellationToken);

                var pagedTransactions = await _unitOfWork.LedgerTransactions.GetByAccountIdAsync(
                    request.AccountId,
                    request.Page,
                    request.PageSize,
                    cancellationToken);

                transactions = pagedTransactions
                    .Select(MapToDto)
                    .ToList();
            }

            var result = new PagedResult<LedgerTransactionDto>(
                transactions,
                totalCount,
                request.Page,
                request.PageSize);

            return Result<PagedResult<LedgerTransactionDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving transaction history for account {AccountId}", request.AccountId);
            return Result<PagedResult<LedgerTransactionDto>>.Failure($"Error retrieving transaction history: {ex.Message}");
        }
    }

    private static LedgerTransactionDto MapToDto(LedgerTransaction transaction)
    {
        return new LedgerTransactionDto
        {
            Id = transaction.Id,
            AccountId = transaction.AccountId,
            TransactionId = transaction.TransactionId.Value,
            Type = transaction.Type.Value,
            Status = transaction.Status.Value,
            Amount = transaction.Amount.Amount,
            Fee = transaction.Fee.Amount,
            Tax = transaction.Tax.Amount,
            BalanceAfter = transaction.BalanceAfter.Amount,
            MerchantId = transaction.MerchantId,
            OriginalTransactionId = transaction.OriginalTransactionId,
            Description = transaction.Description,
            AdjustedBy = transaction.AdjustedBy,
            Reason = transaction.Reason,
            Currency = transaction.Amount.Currency,
            TransactionDate = transaction.TransactionDate,
            CreatedAt = transaction.CreatedAt
        };
    }
}
