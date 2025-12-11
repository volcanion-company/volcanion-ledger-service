using MediatR;
using Volcanion.LedgerService.Application.Common;
using Volcanion.LedgerService.Application.DTOs;

namespace Volcanion.LedgerService.Application.Queries.Accounts;

public record GetAccountByUserIdQuery(string UserId) : IRequest<Result<AccountDto>>;
