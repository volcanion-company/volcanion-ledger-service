using MediatR;
using Volcanion.LedgerService.Application.Common;
using Volcanion.LedgerService.Application.DTOs;

namespace Volcanion.LedgerService.Application.Queries.Accounts;

public record GetAccountByIdQuery(Guid AccountId) : IRequest<Result<AccountDto>>;
