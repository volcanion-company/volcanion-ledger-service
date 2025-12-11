using FluentAssertions;
using Volcanion.LedgerService.Domain.ValueObjects;

namespace Volcanion.LedgerService.Domain.Tests.ValueObjects;

public class TransactionIdTests
{
    [Fact]
    public void Constructor_WithValidValue_ShouldCreateTransactionId()
    {
        // Arrange
        var value = "TXN12345";

        // Act
        var transactionId = new TransactionId(value);

        // Assert
        transactionId.Value.Should().Be(value);
    }

    [Fact]
    public void Constructor_WithEmptyValue_ShouldThrowException()
    {
        // Act
        Action act = () => new TransactionId(string.Empty);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*cannot be empty*");
    }

    [Fact]
    public void Constructor_WithTooLongValue_ShouldThrowException()
    {
        // Arrange
        var value = new string('A', 101);

        // Act
        Action act = () => new TransactionId(value);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*cannot exceed 100 characters*");
    }

    [Fact]
    public void Generate_ShouldCreateUniqueTransactionId()
    {
        // Act
        var txId1 = TransactionId.Generate();
        var txId2 = TransactionId.Generate();

        // Assert
        txId1.Should().NotBe(txId2);
        txId1.Value.Should().NotBeNullOrEmpty();
        txId2.Value.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void FromString_ShouldCreateTransactionId()
    {
        // Arrange
        var value = "TXN12345";

        // Act
        var transactionId = TransactionId.FromString(value);

        // Assert
        transactionId.Value.Should().Be(value);
    }

    [Fact]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        // Arrange
        var txId1 = new TransactionId("TXN12345");
        var txId2 = new TransactionId("TXN12345");

        // Act & Assert
        txId1.Should().Be(txId2);
        (txId1 == txId2).Should().BeTrue();
    }

    [Fact]
    public void ImplicitConversion_ToString_ShouldWork()
    {
        // Arrange
        var transactionId = new TransactionId("TXN12345");

        // Act
        string value = transactionId;

        // Assert
        value.Should().Be("TXN12345");
    }
}
