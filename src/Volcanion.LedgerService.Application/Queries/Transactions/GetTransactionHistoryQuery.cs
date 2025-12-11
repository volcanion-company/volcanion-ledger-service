using MediatR;
using Volcanion.LedgerService.Application.Common;
using Volcanion.LedgerService.Application.DTOs;

namespace Volcanion.LedgerService.Application.Queries.Transactions;

public record GetTransactionHistoryQuery(
    Guid AccountId,
    int Page = 1,
    int PageSize = 50,
    string? TransactionType = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null) : IRequest<Result<PagedResult<LedgerTransactionDto>>>;
