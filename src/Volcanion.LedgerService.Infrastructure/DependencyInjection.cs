using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Volcanion.LedgerService.Domain.Repositories;
using Volcanion.LedgerService.Infrastructure.Persistence;
using Volcanion.LedgerService.Infrastructure.Persistence.Repositories;

namespace Volcanion.LedgerService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database - Write (Primary)
        var writeConnectionString = configuration.GetConnectionString("WriteDatabase")
            ?? throw new InvalidOperationException("WriteDatabase connection string is not configured");

        services.AddDbContext<LedgerDbContext>(options =>
        {
            options.UseNpgsql(writeConnectionString, npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorCodesToAdd: null);
                npgsqlOptions.CommandTimeout(30);
            });

            // Enable sensitive data logging in development
            if (configuration.GetValue<bool>("Logging:EnableSensitiveDataLogging"))
            {
                options.EnableSensitiveDataLogging();
            }
        });

        // Repositories
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<ILedgerTransactionRepository, LedgerTransactionRepository>();
        services.AddScoped<IJournalEntryRepository, JournalEntryRepository>();
        services.AddScoped<IIdempotencyRepository, IdempotencyRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
