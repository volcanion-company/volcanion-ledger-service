using FluentValidation;
using MediatR;

namespace Volcanion.LedgerService.Application.Behaviors;

/// <summary>
/// Implements a pipeline behavior that performs validation on incoming requests using the provided validators before
/// passing the request to the next handler.
/// </summary>
/// <remarks>If any validation errors are detected, a ValidationException is thrown and the request is not
/// processed further. This behavior should be registered in the pipeline to ensure that requests are validated
/// consistently before reaching their handlers.</remarks>
/// <typeparam name="TRequest">The type of the request message to be validated.</typeparam>
/// <typeparam name="TResponse">The type of the response returned by the request handler.</typeparam>
/// <param name="validators">A collection of validators to apply to the request. If no validators are provided, validation is skipped.</param>
public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Validates the incoming request using all registered validators and invokes the next handler in the pipeline if
    /// validation succeeds.
    /// </summary>
    /// <remarks>If no validators are registered for the request type, validation is skipped and the next
    /// handler is invoked directly. All validation errors are collected and reported together in the
    /// exception.</remarks>
    /// <param name="request">The request object to be validated and processed. Cannot be null.</param>
    /// <param name="next">A delegate representing the next handler or behavior to invoke after validation completes.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the validation or request handling operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the response produced by the next
    /// handler in the pipeline.</returns>
    /// <exception cref="ValidationException">Thrown when one or more validation failures are detected in the request.</exception>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // If no validators registered, skip validation
        if (!validators.Any())
        {
            return await next();
        }

        // Create validation context
        var context = new ValidationContext<TRequest>(request);

        // Run all validators
        var validationResults = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        // Collect all failures
        var failures = validationResults
            .Where(r => !r.IsValid)
            .SelectMany(r => r.Errors)
            .ToList();

        // If any validation errors, throw exception
        if (failures.Count != 0)
        {
            throw new ValidationException(failures);
        }

        // Proceed to next behavior/handler
        return await next();
    }
}
