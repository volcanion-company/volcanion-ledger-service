using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volcanion.LedgerService.Domain.Entities;
using Volcanion.LedgerService.Domain.ValueObjects;

namespace Volcanion.LedgerService.Infrastructure.Persistence.Configurations;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("accounts");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(a => a.AccountNumber)
            .HasColumnName("account_number")
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.UserId)
            .HasColumnName("user_id")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.Currency)
            .HasColumnName("currency")
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(a => a.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(a => a.LockedAt)
            .HasColumnName("locked_at");

        builder.Property(a => a.LockedReason)
            .HasColumnName("locked_reason")
            .HasMaxLength(500);

        builder.Property(a => a.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(a => a.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.Property(a => a.RowVersion)
            .HasColumnName("row_version")
            .IsRowVersion();

        // Value Objects - Money
        builder.OwnsOne(a => a.Balance, balance =>
        {
            balance.Property(m => m.Amount)
                .HasColumnName("balance")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            balance.Property(m => m.Currency)
                .HasColumnName("balance_currency")
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.OwnsOne(a => a.AvailableBalance, availableBalance =>
        {
            availableBalance.Property(m => m.Amount)
                .HasColumnName("available_balance")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            availableBalance.Property(m => m.Currency)
                .HasColumnName("available_balance_currency")
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.OwnsOne(a => a.ReservedBalance, reservedBalance =>
        {
            reservedBalance.Property(m => m.Amount)
                .HasColumnName("reserved_balance")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            reservedBalance.Property(m => m.Currency)
                .HasColumnName("reserved_balance_currency")
                .HasMaxLength(3)
                .IsRequired();
        });

        // Indexes
        builder.HasIndex(a => a.AccountNumber)
            .IsUnique()
            .HasDatabaseName("ix_accounts_account_number");

        builder.HasIndex(a => a.UserId)
            .HasDatabaseName("ix_accounts_user_id");

        builder.HasIndex(a => a.CreatedAt)
            .HasDatabaseName("ix_accounts_created_at");

        // Ignore transactions navigation
        builder.Ignore(a => a.Transactions);
    }
}
