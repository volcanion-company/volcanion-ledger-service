using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volcanion.LedgerService.Domain.Entities;
using Volcanion.LedgerService.Domain.ValueObjects;

namespace Volcanion.LedgerService.Infrastructure.Persistence.Configurations;

public class LedgerTransactionConfiguration : IEntityTypeConfiguration<LedgerTransaction>
{
    public void Configure(EntityTypeBuilder<LedgerTransaction> builder)
    {
        builder.ToTable("ledger_transactions");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(t => t.AccountId)
            .HasColumnName("account_id")
            .IsRequired();

        builder.Property(t => t.MerchantId)
            .HasColumnName("merchant_id")
            .HasMaxLength(100);

        builder.Property(t => t.OriginalTransactionId)
            .HasColumnName("original_transaction_id")
            .HasMaxLength(100);

        builder.Property(t => t.Description)
            .HasColumnName("description")
            .HasMaxLength(1000);

        builder.Property(t => t.AdjustedBy)
            .HasColumnName("adjusted_by")
            .HasMaxLength(100);

        builder.Property(t => t.Reason)
            .HasColumnName("reason")
            .HasMaxLength(500);

        builder.Property(t => t.TransactionDate)
            .HasColumnName("transaction_date")
            .IsRequired();

        builder.Property(t => t.Metadata)
            .HasColumnName("metadata")
            .HasColumnType("jsonb");

        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(t => t.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        // Value Objects - TransactionId
        builder.OwnsOne(t => t.TransactionId, transactionId =>
        {
            transactionId.Property(tid => tid.Value)
                .HasColumnName("transaction_id")
                .HasMaxLength(100)
                .IsRequired();
        });

        // Value Objects - TransactionType
        builder.OwnsOne(t => t.Type, type =>
        {
            type.Property(tt => tt.Value)
                .HasColumnName("type")
                .HasMaxLength(50)
                .IsRequired();
        });

        // Value Objects - TransactionStatus
        builder.OwnsOne(t => t.Status, status =>
        {
            status.Property(s => s.Value)
                .HasColumnName("status")
                .HasMaxLength(50)
                .IsRequired();
        });

        // Value Objects - Money (Amount)
        builder.OwnsOne(t => t.Amount, amount =>
        {
            amount.Property(m => m.Amount)
                .HasColumnName("amount")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            amount.Property(m => m.Currency)
                .HasColumnName("amount_currency")
                .HasMaxLength(3)
                .IsRequired();
        });

        // Value Objects - Money (Fee)
        builder.OwnsOne(t => t.Fee, fee =>
        {
            fee.Property(m => m.Amount)
                .HasColumnName("fee")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            fee.Property(m => m.Currency)
                .HasColumnName("fee_currency")
                .HasMaxLength(3)
                .IsRequired();
        });

        // Value Objects - Money (Tax)
        builder.OwnsOne(t => t.Tax, tax =>
        {
            tax.Property(m => m.Amount)
                .HasColumnName("tax")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            tax.Property(m => m.Currency)
                .HasColumnName("tax_currency")
                .HasMaxLength(3)
                .IsRequired();
        });

        // Value Objects - Money (BalanceAfter)
        builder.OwnsOne(t => t.BalanceAfter, balanceAfter =>
        {
            balanceAfter.Property(m => m.Amount)
                .HasColumnName("balance_after")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            balanceAfter.Property(m => m.Currency)
                .HasColumnName("balance_after_currency")
                .HasMaxLength(3)
                .IsRequired();
        });

        // Indexes for performance
        builder.HasIndex(t => t.AccountId)
            .HasDatabaseName("ix_ledger_transactions_account_id");

        builder.HasIndex(t => new { t.AccountId, t.TransactionDate })
            .HasDatabaseName("ix_ledger_transactions_account_date");

        builder.HasIndex("TransactionId_Value")
            .IsUnique()
            .HasDatabaseName("ix_ledger_transactions_transaction_id");

        builder.HasIndex(t => t.MerchantId)
            .HasDatabaseName("ix_ledger_transactions_merchant_id");

        builder.HasIndex(t => t.TransactionDate)
            .HasDatabaseName("ix_ledger_transactions_transaction_date");

        builder.HasIndex("Type_Value")
            .HasDatabaseName("ix_ledger_transactions_type");

        builder.HasIndex("Status_Value")
            .HasDatabaseName("ix_ledger_transactions_status");
    }
}
