using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Logging;
using Volcanion.LedgerService.Domain.Repositories;

namespace Volcanion.LedgerService.Application.Behaviors;

/// <summary>
/// Provides pipeline behavior that enforces idempotency for command requests, ensuring that repeated requests with the
/// same idempotency key return the same response and are not processed multiple times.
/// </summary>
/// <remarks>Idempotency is applied only to command requests; queries are excluded by convention. The idempotency
/// key is extracted from a 'TransactionId' property on the request, if present. If a request with the same idempotency
/// key has already been processed, the cached response is returned. Idempotency records are stored for successful
/// responses and expire after 24 hours. This behavior helps prevent duplicate processing of commands in distributed or
/// retry scenarios.</remarks>
/// <typeparam name="TRequest">The type of the request message. Must implement <see cref="IRequest{TResponse}"/>.</typeparam>
/// <typeparam name="TResponse">The type of the response message returned by the request handler.</typeparam>
/// <param name="idempotencyRepository">The repository used to store and retrieve idempotency records for processed requests.</param>
/// <param name="logger">The logger used to record informational and error messages related to idempotency operations.</param>
public class IdempotencyBehavior<TRequest, TResponse>(
    IIdempotencyRepository idempotencyRepository,
    ILogger<IdempotencyBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Handles the incoming request by applying idempotency for commands, ensuring that duplicate requests with the
    /// same idempotency key return the same response. Queries are processed without idempotency enforcement.
    /// </summary>
    /// <remarks>Idempotency is enforced only for command requests; queries are excluded. If an idempotency
    /// key is present and the request has already been processed, the cached response is returned. Otherwise, the
    /// request is executed and the response is stored for future idempotent calls. The idempotency record is retained
    /// for 24 hours. Exceptions during idempotency record creation are logged but do not affect the request
    /// outcome.</remarks>
    /// <param name="request">The request object to be processed. For commands, an idempotency key may be extracted to determine if the
    /// request has already been handled.</param>
    /// <param name="next">A delegate that invokes the next handler in the pipeline to process the request.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the response to the request, which
    /// may be a cached response if the request is idempotent and has already been processed.</returns>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Only apply idempotency to commands (not queries)
        if (typeof(TRequest).Name.EndsWith("Query"))
        {
            return await next();
        }

        // Extract idempotency key from request
        var idempotencyKey = IdempotencyBehavior<TRequest, TResponse>.GetIdempotencyKey(request);
        
        if (string.IsNullOrEmpty(idempotencyKey))
        {
            // No idempotency key, proceed normally
            return await next();
        }

        // Check if request was already processed
        var existingRecord = await idempotencyRepository.GetByKeyAsync(idempotencyKey, cancellationToken);
        
        if (existingRecord != null)
        {
            // Request already processed, return cached response
            logger.LogInformation(
                "Idempotent request detected: {IdempotencyKey} for {RequestType}",
                idempotencyKey,
                typeof(TRequest).Name);

            if (!string.IsNullOrEmpty(existingRecord.Response))
            {
                var cachedResponse = JsonSerializer.Deserialize<TResponse>(existingRecord.Response);
                if (cachedResponse != null)
                {
                    return cachedResponse;
                }
            }
        }

        // Execute request
        var response = await next();

        // Store idempotency record with response (only for successful operations)
        if (IdempotencyBehavior<TRequest, TResponse>.IsSuccessResponse(response))
        {
            try
            {
                var record = new IdempotencyRecord
                {
                    Id = Guid.NewGuid(),
                    Key = idempotencyKey,
                    Response = JsonSerializer.Serialize(response),
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddHours(24) // 24 hour TTL
                };

                await idempotencyRepository.CreateAsync(record, cancellationToken);

                logger.LogInformation(
                    "Idempotency record created: {IdempotencyKey} for {RequestType}",
                    idempotencyKey,
                    typeof(TRequest).Name);
            }
            catch (Exception ex)
            {
                // Log error but don't fail the request
                logger.LogError(ex,
                    "Failed to create idempotency record for {IdempotencyKey}",
                    idempotencyKey);
            }
        }

        return response;
    }

    /// <summary>
    /// Retrieves an idempotency key from the specified request if it contains a non-null TransactionId property.
    /// </summary>
    /// <remarks>This method relies on the presence of a public TransactionId property on the request type. If
    /// the property is missing or its value is null, the method returns null. The idempotency key can be used to
    /// uniquely identify requests for deduplication or retry scenarios.</remarks>
    /// <param name="request">The request object from which to extract the idempotency key. Must be of a type that defines a TransactionId
    /// property to produce a key.</param>
    /// <returns>A string representing the idempotency key in the format "{TypeName}:{TransactionId}" if the request contains a
    /// non-null TransactionId property; otherwise, null.</returns>
    private static string? GetIdempotencyKey(TRequest request)
    {
        // Try to get idempotency key from TransactionId property
        var transactionIdProperty = typeof(TRequest).GetProperty("TransactionId");
        if (transactionIdProperty != null)
        {
            var value = transactionIdProperty.GetValue(request);
            if (value != null)
            {
                return $"{typeof(TRequest).Name}:{value}";
            }
        }

        return null;
    }

    /// <summary>
    /// Determines whether the specified response object represents a successful result.
    /// </summary>
    /// <remarks>This method is typically used to check the success status of response types that implement an
    /// 'IsSuccess' Boolean property, such as result wrappers. For other types, the method assumes success by
    /// default.</remarks>
    /// <param name="response">The response object to evaluate for success. If the object has an 'IsSuccess' property of type Boolean, its
    /// value is used; otherwise, the response is considered successful by default.</param>
    /// <returns>true if the response is considered successful; otherwise, false.</returns>
    private static bool IsSuccessResponse(TResponse response)
    {
        // Check if response is a Result<T> type with IsSuccess property
        var isSuccessProperty = typeof(TResponse).GetProperty("IsSuccess");
        if (isSuccessProperty != null)
        {
            var isSuccess = isSuccessProperty.GetValue(response);
            return isSuccess is bool success && success;
        }

        // Default to true for non-Result types
        return true;
    }
}
