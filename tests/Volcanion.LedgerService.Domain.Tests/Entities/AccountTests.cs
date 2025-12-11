using FluentAssertions;
using Volcanion.LedgerService.Domain.Entities;
using Volcanion.LedgerService.Domain.Exceptions;
using Volcanion.LedgerService.Domain.ValueObjects;

namespace Volcanion.LedgerService.Domain.Tests.Entities;

public class AccountTests
{
    [Fact]
    public void Create_ShouldCreateAccountWithZeroBalance()
    {
        // Arrange
        var userId = "user123";
        var currency = "VND";

        // Act
        var account = Account.Create(userId, currency);

        // Assert
        account.Should().NotBeNull();
        account.UserId.Should().Be(userId);
        account.Currency.Should().Be(currency);
        account.Balance.Amount.Should().Be(0);
        account.AvailableBalance.Amount.Should().Be(0);
        account.ReservedBalance.Amount.Should().Be(0);
        account.IsActive.Should().BeTrue();
        account.AccountNumber.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Topup_ShouldIncreaseBalance()
    {
        // Arrange
        var account = Account.Create("user123", "VND");
        var amount = new Money(100.50m, "VND");
        var transactionId = TransactionId.Generate();

        // Act
        account.Topup(amount, transactionId, "Test topup");

        // Assert
        account.Balance.Amount.Should().Be(100.50m);
        account.AvailableBalance.Amount.Should().Be(100.50m);
        account.Transactions.Should().HaveCount(1);
        account.Transactions.First().Type.Should().Be(TransactionType.Topup);
    }

    [Fact]
    public void ProcessPayment_WithSufficientBalance_ShouldDecreaseBalance()
    {
        // Arrange
        var account = Account.Create("user123", "VND");
        var topupAmount = new Money(1000m, "VND");
        account.Topup(topupAmount, TransactionId.Generate());

        var paymentAmount = new Money(100m, "VND");
        var fee = new Money(5m, "VND");
        var tax = new Money(10m, "VND");
        var transactionId = TransactionId.Generate();

        // Act
        account.ProcessPayment(paymentAmount, fee, tax, transactionId, "merchant123", "Test payment");

        // Assert
        var expectedBalance = 1000m - 100m - 5m - 10m; // 885
        account.Balance.Amount.Should().Be(expectedBalance);
        account.AvailableBalance.Amount.Should().Be(expectedBalance);
        account.Transactions.Should().HaveCount(2);
        account.Transactions.Last().Type.Should().Be(TransactionType.Payment);
    }

    [Fact]
    public void ProcessPayment_WithInsufficientBalance_ShouldThrowException()
    {
        // Arrange
        var account = Account.Create("user123", "VND");
        var topupAmount = new Money(50m, "VND");
        account.Topup(topupAmount, TransactionId.Generate());

        var paymentAmount = new Money(100m, "VND");
        var fee = new Money(5m, "VND");
        var tax = new Money(10m, "VND");
        var transactionId = TransactionId.Generate();

        // Act
        Action act = () => account.ProcessPayment(
            paymentAmount, fee, tax, transactionId, "merchant123");

        // Assert
        act.Should().Throw<InsufficientBalanceException>()
            .WithMessage("*insufficient balance*");
    }

    [Fact]
    public void ProcessRefund_ShouldIncreaseBalance()
    {
        // Arrange
        var account = Account.Create("user123", "VND");
        var topupAmount = new Money(1000m, "VND");
        account.Topup(topupAmount, TransactionId.Generate());

        var paymentAmount = new Money(100m, "VND");
        var paymentTxId = TransactionId.Generate();
        account.ProcessPayment(
            paymentAmount,
            Money.Zero("VND"),
            Money.Zero("VND"),
            paymentTxId,
            "merchant123");

        var refundAmount = new Money(50m, "VND");
        var refundTxId = TransactionId.Generate();

        // Act
        account.ProcessRefund(refundAmount, refundTxId, paymentTxId, "Partial refund");

        // Assert
        var expectedBalance = 1000m - 100m + 50m; // 950
        account.Balance.Amount.Should().Be(expectedBalance);
        account.Transactions.Should().HaveCount(3);
        account.Transactions.Last().Type.Should().Be(TransactionType.Refund);
    }

    [Fact]
    public void ApplyAdjustment_WithPositiveAmount_ShouldIncreaseBalance()
    {
        // Arrange
        var account = Account.Create("user123", "VND");
        var adjustmentAmount = new Money(500m, "VND");
        var transactionId = TransactionId.Generate();

        // Act
        account.ApplyAdjustment(
            adjustmentAmount,
            transactionId,
            "Manual balance correction",
            "admin@system.com");

        // Assert
        account.Balance.Amount.Should().Be(500m);
        account.Transactions.Should().HaveCount(1);
        account.Transactions.First().Type.Should().Be(TransactionType.Adjustment);
    }

    [Fact]
    public void ApplyAdjustment_WithNegativeAmount_ShouldThrowException()
    {
        // Arrange
        var account = Account.Create("user123", "VND");

        // Act
        Action act = () => new Money(-100m, "VND");

        // Assert
        act.Should().Throw<InvalidMoneyException>();
    }

    [Fact]
    public void ReserveBalance_WithSufficientBalance_ShouldDecreaseAvailableBalance()
    {
        // Arrange
        var account = Account.Create("user123", "VND");
        var topupAmount = new Money(1000m, "VND");
        account.Topup(topupAmount, TransactionId.Generate());

        var reserveAmount = new Money(100m, "VND");

        // Act
        account.ReserveBalance(reserveAmount);

        // Assert
        account.Balance.Amount.Should().Be(1000m);
        account.AvailableBalance.Amount.Should().Be(900m);
        account.ReservedBalance.Amount.Should().Be(100m);
    }

    [Fact]
    public void Lock_ShouldDeactivateAccount()
    {
        // Arrange
        var account = Account.Create("user123", "VND");

        // Act
        account.Lock("Suspicious activity");

        // Assert
        account.IsActive.Should().BeFalse();
        account.LockedAt.Should().NotBeNull();
        account.LockedReason.Should().Be("Suspicious activity");
    }

    [Fact]
    public void ProcessPayment_OnLockedAccount_ShouldThrowException()
    {
        // Arrange
        var account = Account.Create("user123", "VND");
        var topupAmount = new Money(1000m, "VND");
        account.Topup(topupAmount, TransactionId.Generate());
        account.Lock("Test lock");

        var paymentAmount = new Money(100m, "VND");

        // Act
        Action act = () => account.ProcessPayment(
            paymentAmount,
            Money.Zero("VND"),
            Money.Zero("VND"),
            TransactionId.Generate(),
            "merchant123");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*locked*");
    }

    [Fact]
    public void Unlock_ShouldReactivateAccount()
    {
        // Arrange
        var account = Account.Create("user123", "VND");
        account.Lock("Test lock");

        // Act
        account.Unlock();

        // Assert
        account.IsActive.Should().BeTrue();
        account.LockedAt.Should().BeNull();
        account.LockedReason.Should().BeNull();
    }
}
