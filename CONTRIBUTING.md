# Contributing to Volcanion Ledger Service

Thank you for your interest in contributing to the Volcanion Ledger Service! This document provides guidelines and instructions for contributing to this project.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Workflow](#development-workflow)
- [Coding Standards](#coding-standards)
- [Testing Guidelines](#testing-guidelines)
- [Pull Request Process](#pull-request-process)
- [Issue Reporting](#issue-reporting)

## Code of Conduct

By participating in this project, you agree to maintain a respectful and inclusive environment for all contributors.

### Our Standards

- Use welcoming and inclusive language
- Be respectful of differing viewpoints and experiences
- Accept constructive criticism gracefully
- Focus on what is best for the community
- Show empathy towards other community members

## Getting Started

### Prerequisites

Before contributing, ensure you have:

- .NET 8.0 SDK or later
- PostgreSQL 15+ (or Docker)
- Git
- Your favorite IDE (Visual Studio, VS Code, or Rider)

### Setting Up Your Development Environment

1. **Fork the repository**
   ```bash
   git clone https://github.com/volcanion-company/volcanion-ledger-service.git
   cd volcanion-ledger-service
   ```

2. **Create a branch for your feature**
   ```bash
   git checkout -b feature/your-feature-name
   ```

3. **Set up the database**
   - See [docs/SETUP.md](docs/SETUP.md) for detailed instructions

4. **Build the solution**
   ```bash
   dotnet build
   ```

5. **Run tests**
   ```bash
   dotnet test
   ```

## Development Workflow

### Branch Naming Convention

- `feature/feature-name` - New features
- `fix/bug-description` - Bug fixes
- `refactor/description` - Code refactoring
- `docs/description` - Documentation updates
- `test/description` - Test additions or modifications

### Commit Messages

Follow the [Conventional Commits](https://www.conventionalcommits.org/) specification:

```
<type>(<scope>): <subject>

<body>

<footer>
```

**Types:**
- `feat`: New feature
- `fix`: Bug fix
- `refactor`: Code refactoring
- `test`: Adding or updating tests
- `docs`: Documentation changes
- `chore`: Maintenance tasks

**Examples:**
```
feat(accounts): add account locking functionality

Implement account lock/unlock methods with reason tracking.
Includes validation and audit logging.

Closes #123
```

```
fix(transactions): prevent race condition in payment processing

Add pessimistic locking using SELECT FOR UPDATE to prevent
concurrent modifications to account balance.

Fixes #456
```

## Coding Standards

### C# Style Guidelines

We follow the [Microsoft C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions) with additional project-specific rules:

#### General Principles

- **Clean Architecture**: Maintain strict separation between layers
  - Domain: Business logic and entities
  - Application: Use cases and command/query handlers
  - Infrastructure: External dependencies (database, external APIs)
  - API: Controllers and middleware

- **SOLID Principles**: Follow SOLID design principles
- **DRY**: Don't Repeat Yourself
- **KISS**: Keep It Simple, Stupid

#### Naming Conventions

```csharp
// Classes and Methods: PascalCase
public class AccountRepository
{
    public async Task<Account> GetByIdAsync(Guid id) { }
}

// Private fields: _camelCase
private readonly ILogger<AccountService> _logger;

// Parameters and local variables: camelCase
public void ProcessPayment(decimal amount, string merchantId)
{
    var totalCost = amount + fee;
}

// Constants: UPPER_SNAKE_CASE or PascalCase
private const int MAX_RETRY_ATTEMPTS = 3;
```

#### Code Organization

```csharp
// 1. Using statements (organized by System -> Third-party -> Project)
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Volcanion.LedgerService.Domain.Entities;

// 2. Namespace
namespace Volcanion.LedgerService.Application.Commands;

// 3. Class definition with XML documentation
/// <summary>
/// Handles payment processing commands
/// </summary>
public class ProcessPaymentCommandHandler
{
    // 4. Private readonly fields
    private readonly ILogger<ProcessPaymentCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    // 5. Constructor
    public ProcessPaymentCommandHandler(
        ILogger<ProcessPaymentCommandHandler> logger,
        IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    // 6. Public methods
    // 7. Private methods
}
```

#### Domain-Driven Design Patterns

- **Entities**: Rich domain models with business logic
- **Value Objects**: Immutable objects (Money, TransactionId)
- **Aggregates**: Account is the aggregate root
- **Repository Pattern**: Data access abstraction
- **Unit of Work**: Transaction management

#### Error Handling

```csharp
// Use domain exceptions for business rule violations
if (amount <= 0)
    throw new InvalidMoneyException(amount, "Amount must be positive");

// Use Result pattern for operation outcomes
return Result<AccountDto>.Success(dto);
return Result<AccountDto>.Failure("Account not found");
```

### Testing Standards

#### Unit Tests

```csharp
[Fact]
public void Topup_WithPositiveAmount_IncreasesBalance()
{
    // Arrange
    var account = Account.Create("user123");
    var amount = new Money(100m, "VND");

    // Act
    account.Topup(amount, TransactionId.Generate());

    // Assert
    Assert.Equal(100m, account.Balance.Amount);
}
```

#### Integration Tests

- Test complete workflows
- Use test containers for database
- Clean up after each test

#### Test Coverage

- Aim for minimum 80% code coverage
- 100% coverage for domain entities
- Test both happy paths and error scenarios

## Testing Guidelines

### Running Tests

```bash
# Run all tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test tests/Volcanion.LedgerService.Domain.Tests
```

### Writing Tests

- **Arrange-Act-Assert** pattern for clarity
- One assertion per test when possible
- Descriptive test names: `MethodName_Scenario_ExpectedBehavior`
- Use test data builders for complex objects

## Pull Request Process

### Before Submitting

1. **Update your branch**
   ```bash
   git fetch origin
   git rebase origin/main
   ```

2. **Run all tests**
   ```bash
   dotnet test
   ```

3. **Check code formatting**
   ```bash
   dotnet format
   ```

4. **Update documentation** if needed

### Pull Request Guidelines

1. **Title**: Clear and descriptive
   - Good: `feat(accounts): add account locking with reason tracking`
   - Bad: `fix bug`

2. **Description**: Include
   - What changes were made
   - Why the changes were necessary
   - How to test the changes
   - Screenshots (if UI changes)
   - Related issues

3. **Review Checklist**
   - [ ] Code follows project style guidelines
   - [ ] Tests added/updated and passing
   - [ ] Documentation updated
   - [ ] No breaking changes (or documented if necessary)
   - [ ] Commits are clean and well-organized

### Pull Request Template

```markdown
## Description
Brief description of changes

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Breaking change
- [ ] Documentation update

## Testing
Describe testing performed

## Checklist
- [ ] Tests pass locally
- [ ] Code follows style guidelines
- [ ] Documentation updated
- [ ] No new warnings
```

## Issue Reporting

### Bug Reports

Include:
- Clear title and description
- Steps to reproduce
- Expected vs actual behavior
- Environment details (.NET version, OS, database)
- Stack traces if applicable
- Screenshots if relevant

### Feature Requests

Include:
- Clear use case
- Expected behavior
- Proposed implementation (optional)
- Alternatives considered

### Issue Labels

- `bug`: Something isn't working
- `enhancement`: New feature or request
- `documentation`: Documentation improvements
- `good first issue`: Good for newcomers
- `help wanted`: Extra attention needed
- `priority:high`: High priority
- `priority:medium`: Medium priority
- `priority:low`: Low priority

## Project Structure

```
volcanion-ledger-service/
├── src/
│   ├── Volcanion.LedgerService.API/           # Web API layer
│   ├── Volcanion.LedgerService.Application/   # Use cases & handlers
│   ├── Volcanion.LedgerService.Domain/        # Domain entities & logic
│   └── Volcanion.LedgerService.Infrastructure/ # External dependencies
├── tests/
│   └── Volcanion.LedgerService.Domain.Tests/  # Unit tests
├── docs/                                       # Documentation
└── docker-compose.yml                          # Docker setup
```

## Key Architectural Decisions

### Transaction Safety

- **Single SaveChanges**: One commit per transaction
- **Pessimistic Locking**: `SELECT FOR UPDATE` for balance operations
- **Idempotency**: Request deduplication based on TransactionId

### Validation Strategy

- **MediatR Pipeline**: Automatic validation using FluentValidation
- **Domain Validation**: Business rules enforced in entities
- **API Validation**: Input sanitization in controllers

### Error Handling

- **RFC 7807 Compliance**: Standard ProblemDetails responses
- **Domain Exceptions**: Business rule violations
- **Validation Exceptions**: Input validation errors

## Getting Help

- **Documentation**: Check [docs/](docs/) folder
- **Issues**: Search existing issues or create new one
- **Discussions**: Use GitHub Discussions for questions

## License

By contributing, you agree that your contributions will be licensed under the MIT License.

## Recognition

Contributors will be recognized in the project README. Thank you for helping improve the Volcanion Ledger Service!
