using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volcanion.LedgerService.Domain.Entities;

namespace Volcanion.LedgerService.Infrastructure.Persistence.Configurations;

public class JournalEntryConfiguration : IEntityTypeConfiguration<JournalEntry>
{
    public void Configure(EntityTypeBuilder<JournalEntry> builder)
    {
        builder.ToTable("journal_entries");

        builder.HasKey(j => j.Id);

        builder.Property(j => j.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(j => j.LedgerTransactionId)
            .HasColumnName("ledger_transaction_id")
            .IsRequired();

        builder.Property(j => j.AccountId)
            .HasColumnName("account_id")
            .IsRequired();

        builder.Property(j => j.AccountType)
            .HasColumnName("account_type")
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(j => j.Description)
            .HasColumnName("description")
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(j => j.EntryDate)
            .HasColumnName("entry_date")
            .IsRequired();

        builder.Property(j => j.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(j => j.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        // Value Objects - Money (Amount)
        builder.OwnsOne(j => j.Amount, amount =>
        {
            amount.Property(m => m.Amount)
                .HasColumnName("amount")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            amount.Property(m => m.Currency)
                .HasColumnName("currency")
                .HasMaxLength(3)
                .IsRequired();
        });

        // Indexes
        builder.HasIndex(j => j.LedgerTransactionId)
            .HasDatabaseName("ix_journal_entries_ledger_transaction_id");

        builder.HasIndex(j => j.AccountId)
            .HasDatabaseName("ix_journal_entries_account_id");

        builder.HasIndex(j => j.EntryDate)
            .HasDatabaseName("ix_journal_entries_entry_date");

        builder.HasIndex(j => j.AccountType)
            .HasDatabaseName("ix_journal_entries_account_type");
    }
}
