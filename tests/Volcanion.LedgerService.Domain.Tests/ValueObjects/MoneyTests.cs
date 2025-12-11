using FluentAssertions;
using Volcanion.LedgerService.Domain.Exceptions;
using Volcanion.LedgerService.Domain.ValueObjects;

namespace Volcanion.LedgerService.Domain.Tests.ValueObjects;

public class MoneyTests
{
    [Fact]
    public void Constructor_WithValidAmount_ShouldCreateMoney()
    {
        // Arrange & Act
        var money = new Money(100.50m, "VND");

        // Assert
        money.Amount.Should().Be(100.50m);
        money.Currency.Should().Be("VND");
    }

    [Fact]
    public void Constructor_WithNegativeAmount_ShouldThrowException()
    {
        // Act
        Action act = () => new Money(-10m, "VND");

        // Assert
        act.Should().Throw<InvalidMoneyException>()
            .WithMessage("*cannot be negative*");
    }

    [Fact]
    public void Constructor_WithMoreThanTwoDecimals_ShouldThrowException()
    {
        // Act
        Action act = () => new Money(100.555m, "VND");

        // Assert
        act.Should().Throw<InvalidMoneyException>()
            .WithMessage("*2 decimal places*");
    }

    [Fact]
    public void Add_WithSameCurrency_ShouldReturnSum()
    {
        // Arrange
        var money1 = new Money(100m, "VND");
        var money2 = new Money(50m, "VND");

        // Act
        var result = money1.Add(money2);

        // Assert
        result.Amount.Should().Be(150m);
        result.Currency.Should().Be("VND");
    }

    [Fact]
    public void Add_WithDifferentCurrency_ShouldThrowException()
    {
        // Arrange
        var money1 = new Money(100m, "VND");
        var money2 = new Money(50m, "EUR");

        // Act
        Action act = () => money1.Add(money2);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*different currencies*");
    }

    [Fact]
    public void Subtract_WithSameCurrency_ShouldReturnDifference()
    {
        // Arrange
        var money1 = new Money(100m, "VND");
        var money2 = new Money(30m, "VND");

        // Act
        var result = money1.Subtract(money2);

        // Assert
        result.Amount.Should().Be(70m);
        result.Currency.Should().Be("VND");
    }

    [Fact]
    public void Subtract_ResultingInNegative_ShouldThrowException()
    {
        // Arrange
        var money1 = new Money(50m, "VND");
        var money2 = new Money(100m, "VND");

        // Act
        Action act = () => money1.Subtract(money2);

        // Assert
        act.Should().Throw<InvalidMoneyException>()
            .WithMessage("*cannot be negative*");
    }

    [Fact]
    public void IsGreaterThanOrEqual_ShouldReturnCorrectResult()
    {
        // Arrange
        var money1 = new Money(100m, "VND");
        var money2 = new Money(50m, "VND");
        var money3 = new Money(100m, "VND");

        // Act & Assert
        money1.IsGreaterThanOrEqual(money2).Should().BeTrue();
        money1.IsGreaterThanOrEqual(money3).Should().BeTrue();
        money2.IsGreaterThanOrEqual(money1).Should().BeFalse();
    }

    [Fact]
    public void Zero_ShouldCreateZeroMoney()
    {
        // Act
        var money = Money.Zero("VND");

        // Assert
        money.Amount.Should().Be(0m);
        money.Currency.Should().Be("VND");
    }

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        // Arrange
        var money1 = new Money(100m, "VND");
        var money2 = new Money(100m, "VND");

        // Act & Assert
        money1.Should().Be(money2);
        (money1 == money2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentValues_ShouldReturnFalse()
    {
        // Arrange
        var money1 = new Money(100m, "VND");
        var money2 = new Money(50m, "VND");

        // Act & Assert
        money1.Should().NotBe(money2);
        (money1 == money2).Should().BeFalse();
    }

    [Fact]
    public void OperatorAdd_ShouldWork()
    {
        // Arrange
        var money1 = new Money(100m, "VND");
        var money2 = new Money(50m, "VND");

        // Act
        var result = money1 + money2;

        // Assert
        result.Amount.Should().Be(150m);
    }

    [Fact]
    public void OperatorSubtract_ShouldWork()
    {
        // Arrange
        var money1 = new Money(100m, "VND");
        var money2 = new Money(30m, "VND");

        // Act
        var result = money1 - money2;

        // Assert
        result.Amount.Should().Be(70m);
    }

    [Fact]
    public void OperatorGreaterThan_ShouldWork()
    {
        // Arrange
        var money1 = new Money(100m, "VND");
        var money2 = new Money(50m, "VND");

        // Act & Assert
        (money1 > money2).Should().BeTrue();
        (money2 > money1).Should().BeFalse();
    }
}
