using System.Net;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Volcanion.LedgerService.Domain.Exceptions;

namespace Volcanion.LedgerService.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

        var problemDetails = exception switch
        {
            ValidationException validationException => new ValidationProblemDetails(
                validationException.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    ))
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "Validation Error",
                Status = StatusCodes.Status400BadRequest,
                Detail = "One or more validation errors occurred.",
                Instance = context.Request.Path
            },

            InsufficientBalanceException insufficientBalance => new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "Insufficient Balance",
                Status = StatusCodes.Status400BadRequest,
                Detail = insufficientBalance.Message,
                Instance = context.Request.Path,
                Extensions =
                {
                    ["accountId"] = insufficientBalance.AccountId.ToString(),
                    ["requiredAmount"] = insufficientBalance.RequiredAmount,
                    ["availableBalance"] = insufficientBalance.AvailableBalance
                }
            },

            NegativeBalanceNotAllowedException negativeBalance => new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "Negative Balance Not Allowed",
                Status = StatusCodes.Status400BadRequest,
                Detail = negativeBalance.Message,
                Instance = context.Request.Path,
                Extensions =
                {
                    ["accountId"] = negativeBalance.AccountId.ToString(),
                    ["attemptedBalance"] = negativeBalance.AttemptedBalance
                }
            },

            InvalidMoneyException invalidMoney => new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "Invalid Money Amount",
                Status = StatusCodes.Status400BadRequest,
                Detail = invalidMoney.Message,
                Instance = context.Request.Path,
                Extensions =
                {
                    ["amount"] = invalidMoney.Amount
                }
            },

            DuplicateTransactionException duplicateTransaction => new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.8",
                Title = "Duplicate Transaction",
                Status = StatusCodes.Status409Conflict,
                Detail = duplicateTransaction.Message,
                Instance = context.Request.Path,
                Extensions =
                {
                    ["transactionId"] = duplicateTransaction.TransactionId
                }
            },

            DomainException domainException => new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "Domain Error",
                Status = StatusCodes.Status400BadRequest,
                Detail = domainException.Message,
                Instance = context.Request.Path
            },

            _ => new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                Title = "Internal Server Error",
                Status = StatusCodes.Status500InternalServerError,
                Detail = "An error occurred while processing your request.",
                Instance = context.Request.Path
            }
        };

        // Add trace ID to all responses
        problemDetails.Extensions["traceId"] = context.TraceIdentifier;

        context.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(problemDetails);
    }
}
