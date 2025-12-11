# Architecture - Volcanion Ledger Service

This document describes the architecture, design patterns, and technical decisions of the Volcanion Ledger Service.

## Table of Contents

- [Overview](#overview)
- [Architecture Patterns](#architecture-patterns)
- [System Components](#system-components)
- [Data Flow](#data-flow)
- [Design Decisions](#design-decisions)
- [Database Design](#database-design)
- [Security & Reliability](#security--reliability)
- [Performance Considerations](#performance-considerations)

## Overview

### Purpose

The Volcanion Ledger Service is a production-grade financial ledger system designed for payment gateways and e-wallet applications. It provides:

- **Account Management**: Multi-currency account creation and management
- **Transaction Processing**: Topup, payment, refund, and adjustment operations
- **Double-Entry Bookkeeping**: Complete audit trail with journal entries
- **High Reliability**: ACID transactions, idempotency, and concurrency control
- **Observability**: Structured logging, metrics, and health checks

### Key Features

- ✅ **ACID Compliance**: Database transactions ensure data consistency
- ✅ **Idempotent Operations**: Duplicate request detection prevents double-charging
- ✅ **Concurrency Control**: Pessimistic locking prevents race conditions
- ✅ **Audit Trail**: Complete transaction history with journal entries
- ✅ **Validation**: Automatic request validation using FluentValidation
- ✅ **Error Handling**: RFC 7807 compliant error responses
- ✅ **Monitoring**: Prometheus metrics for production observability

## Architecture Patterns

### Clean Architecture

The solution follows Clean Architecture principles with clear separation of concerns:

```
┌─────────────────────────────────────────────────────────┐
│                    API Layer                            │
│  - Controllers (HTTP endpoints)                         │
│  - Middleware (logging, error handling, metrics)        │
│  - DTOs (data transfer objects)                         │
└────────────────────┬────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────┐
│                Application Layer                        │
│  - Commands & Queries (CQRS)                           │
│  - Command/Query Handlers (MediatR)                    │
│  - Validation (FluentValidation)                       │
│  - Business workflows                                   │
└────────────────────┬────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────┐
│                  Domain Layer                           │
│  - Entities (Account, Transaction, JournalEntry)       │
│  - Value Objects (Money, TransactionId)                │
│  - Domain Logic & Business Rules                       │
│  - Repository Interfaces                               │
└────────────────────┬────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────┐
│               Infrastructure Layer                       │
│  - Repository Implementations (EF Core)                 │
│  - Database Context & Migrations                        │
│  - External Service Integrations                        │
└─────────────────────────────────────────────────────────┘
```

**Benefits:**
- **Independence**: Domain logic independent of frameworks and databases
- **Testability**: Each layer can be tested in isolation
- **Maintainability**: Clear boundaries make code easier to understand
- **Flexibility**: Easy to swap implementations (e.g., change database)

### CQRS (Command Query Responsibility Segregation)

Separates read and write operations for better scalability and clarity:

```
Commands (Write)          Queries (Read)
─────────────────         ──────────────
CreateAccountCommand      GetAccountByIdQuery
TopupCommand             GetAccountsByUserQuery
ProcessPaymentCommand    GetTransactionHistoryQuery
ProcessRefundCommand
ApplyAdjustmentCommand
```

**Commands:**
- Modify state
- Return simple success/failure results
- Validated and processed through MediatR pipeline

**Queries:**
- Read-only operations
- Return DTOs (data transfer objects)
- Can be optimized independently (caching, read replicas)

### MediatR Pipeline

Request processing flows through a pipeline of behaviors:

```
Request → IdempotencyBehavior → ValidationBehavior → Handler → Response
          ─────────────────────────────────────────────────────
          │                                                    │
          │ 1. Check for duplicate request                    │
          │ 2. Validate input                                 │
          │ 3. Execute business logic                         │
          │ 4. Return result                                  │
          └────────────────────────────────────────────────────┘
```

**Pipeline Behaviors:**

1. **IdempotencyBehavior** (First)
   - Checks if request was processed before
   - Returns cached response for duplicates
   - Caches successful responses for 24 hours

2. **ValidationBehavior** (Second)
   - Runs FluentValidation validators
   - Throws `ValidationException` on failure
   - Automatic, no manual validation needed

3. **Command/Query Handler** (Last)
   - Executes business logic
   - Returns Result<T> or DTO

## System Components

### 1. API Layer

**Controllers:**
- `AccountsController`: Account management endpoints
- `TransactionsController`: Transaction processing endpoints

**Middleware:**
- `RequestLoggingMiddleware`: Logs all HTTP requests
- `ExceptionHandlingMiddleware`: Global error handling with RFC 7807
- `MetricsMiddleware`: Records request duration and status codes

**Metrics:**
- `LedgerMetrics`: Prometheus metrics for monitoring

### 2. Application Layer

**Commands (Write Operations):**

| Command | Purpose | Validation Rules |
|---------|---------|------------------|
| `CreateAccountCommand` | Create new account | UserId required, valid currency |
| `TopupCommand` | Add money | Amount > 0, account exists |
| `ProcessPaymentCommand` | Deduct money | Amount > 0, sufficient balance |
| `ProcessRefundCommand` | Return money | Amount > 0, original transaction exists |
| `ApplyAdjustmentCommand` | Manual correction | Amount > 0, reason required |

**Queries (Read Operations):**

| Query | Purpose | Parameters |
|-------|---------|------------|
| `GetAccountByIdQuery` | Get account details | accountId |
| `GetAccountsByUserQuery` | Get user's accounts | userId |
| `GetTransactionHistoryQuery` | Get transaction list | accountId, pagination |
| `GetTransactionByIdQuery` | Get transaction details | transactionId |

**Validators:**
- Each command has a corresponding FluentValidator
- Validators run automatically via `ValidationBehavior`

### 3. Domain Layer

**Entities:**

**Account** (Aggregate Root)
```csharp
public class Account : AggregateRoot
{
    public string AccountNumber { get; }
    public string UserId { get; }
    public Money Balance { get; }
    public Money AvailableBalance { get; }
    public Money ReservedBalance { get; }
    public string Currency { get; }
    public bool IsActive { get; }
    
    // Business methods
    public void Topup(Money amount, TransactionId txnId);
    public void ProcessPayment(Money amount, Money fee, Money tax, ...);
    public void ProcessRefund(Money amount, ...);
    public void ApplyAdjustment(Money amount, ...);
    public void Lock(string reason);
    public void Unlock();
}
```

**LedgerTransaction**
- Immutable record of transaction
- Append-only (never updated)
- Tracks balance after transaction

**JournalEntry**
- Double-entry bookkeeping record
- Every transaction creates 2+ journal entries
- Debits must equal credits

**Value Objects:**

**Money**
```csharp
public class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }
    
    // Immutable operations
    public Money Add(Money other);
    public Money Subtract(Money other);
    public bool IsLessThan(Money other);
}
```

**TransactionId**
- Strongly-typed transaction identifier
- Used for idempotency

### 4. Infrastructure Layer

**Repositories:**
- `AccountRepository`: Account data access
- `LedgerTransactionRepository`: Transaction data access
- `JournalEntryRepository`: Journal entry data access
- `IdempotencyRepository`: Idempotency cache

**Unit of Work:**
```csharp
public interface IUnitOfWork
{
    IAccountRepository Accounts { get; }
    ILedgerTransactionRepository Transactions { get; }
    IJournalEntryRepository JournalEntries { get; }
    
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
```

**Database Context:**
- EF Core `LedgerDbContext`
- PostgreSQL database
- Supports migrations

## Data Flow

### Example: Process Payment Flow

```
1. HTTP Request
   POST /api/v1/transactions/payment
   {
     "accountId": "...",
     "amount": 100000,
     "fee": 2000,
     "tax": 1000,
     "transactionId": "TXN001"
   }

2. Controller
   TransactionsController.ProcessPayment()
   → Sends ProcessPaymentCommand to MediatR

3. IdempotencyBehavior
   → Check if TXN001 was processed before
   → If yes: return cached response
   → If no: continue to validation

4. ValidationBehavior
   → Validate amount > 0
   → Validate account exists
   → Validate sufficient balance
   → If invalid: return 400 error

5. ProcessPaymentCommandHandler
   a. Begin database transaction
   b. Lock account (SELECT FOR UPDATE)
   c. Validate business rules
   d. Call account.ProcessPayment()
   e. Update account
   f. Create journal entries:
      - CREDIT: Customer account (-103,000)
      - DEBIT: Merchant (+100,000)
      - DEBIT: Fee revenue (+2,000)
      - DEBIT: Tax revenue (+1,000)
   g. Validate debits = credits
   h. Save journal entries
   i. Commit database transaction
   j. Cache response (idempotency)

6. Response
   200 OK
   {
     "id": "...",
     "transactionId": "TXN001",
     "type": "Payment",
     "status": "Completed",
     "balanceAfter": 897000
   }
```

### Database Transaction Flow

```
┌─────────────────────────────────────────────┐
│  BEGIN TRANSACTION                          │
├─────────────────────────────────────────────┤
│  1. SELECT ... FOR UPDATE (lock account)    │
│  2. Validate business rules                 │
│  3. Update account balance                  │
│  4. Insert ledger_transaction               │
│  5. Insert journal_entries (2+)             │
│  6. Validate SUM(debits) = SUM(credits)     │
├─────────────────────────────────────────────┤
│  COMMIT TRANSACTION                         │
└─────────────────────────────────────────────┘
```

## Design Decisions

### 1. Single SaveChanges per Transaction

**Decision:** Each business transaction commits once via `UnitOfWork.CommitTransactionAsync()`.

**Rationale:**
- Prevents partial commits
- Ensures atomicity
- Simpler error handling

**Before (problematic):**
```csharp
account.Topup(amount);
await _context.SaveChangesAsync(); // ❌ Partial commit
await _journalRepo.AddAsync(entries);
await _context.SaveChangesAsync(); // ❌ If this fails, money added but no journal
```

**After (correct):**
```csharp
account.Topup(amount);
_context.Accounts.Update(account);
await _journalRepo.AddRangeAsync(entries);
await _unitOfWork.CommitTransactionAsync(); // ✅ All or nothing
```

### 2. Pessimistic Locking

**Decision:** Use `SELECT FOR UPDATE` for all balance-changing operations.

**Rationale:**
- Prevents race conditions
- Simple to reason about
- Suitable for financial operations

**Implementation:**
```csharp
public async Task<Account?> GetByIdWithLockAsync(Guid id)
{
    return await _context.Accounts
        .FromSqlRaw("SELECT * FROM accounts WHERE id = {0} FOR UPDATE", id)
        .FirstOrDefaultAsync();
}
```

**Why not optimistic locking?**
- Optimistic locking (RowVersion) detects conflicts after the fact
- Pessimistic locking prevents conflicts upfront
- For financial operations, blocking is acceptable

**Note:** We still include `RowVersion` for additional safety in case of bugs.

### 3. Idempotency via TransactionId

**Decision:** All commands require unique `transactionId`, responses cached for 24 hours.

**Rationale:**
- Prevents duplicate charges from network retries
- Client can safely retry on timeout
- Improves reliability in distributed systems

**Implementation:**
```csharp
// IdempotencyBehavior checks cache before executing
var cacheKey = $"{request.GetType().Name}:{transactionId}";
var cached = await _repository.GetAsync(cacheKey);
if (cached != null)
    return cached; // Return original response

// Execute command...

// Cache successful response
await _repository.SetAsync(cacheKey, response, TimeSpan.FromHours(24));
```

### 4. Validation Pipeline

**Decision:** Use MediatR behavior for automatic validation.

**Rationale:**
- DRY: No manual validation in handlers
- Consistent validation across all commands
- Separation of concerns

**Implementation:**
```csharp
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    public async Task<TResponse> Handle(TRequest request, ...)
    {
        var failures = _validators
            .Select(v => v.Validate(context))
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Any())
            throw new ValidationException(failures);

        return await next();
    }
}
```

### 5. RFC 7807 Error Responses

**Decision:** Use ASP.NET Core built-in `ProblemDetails` for all errors.

**Rationale:**
- Industry standard (RFC 7807)
- Consistent error format
- Better client experience

**Example:**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Insufficient Balance",
  "status": 400,
  "detail": "Account does not have sufficient balance",
  "extensions": {
    "accountId": "123",
    "required": 100000,
    "available": 50000,
    "traceId": "00-abc..."
  }
}
```

### 6. No Domain Events (Removed)

**Decision:** Remove domain events infrastructure.

**Rationale:**
- Not currently used
- Adds complexity without benefit
- Can be re-added when needed (YAGNI principle)

**Alternative:** Use application-level events if needed later.

## Database Design

### Schema Overview

```
┌─────────────────┐       ┌──────────────────────┐
│    accounts     │       │ ledger_transactions  │
├─────────────────┤       ├──────────────────────┤
│ id (PK)         │◄──────┤ account_id (FK)      │
│ account_number  │       │ transaction_id       │
│ user_id         │       │ type                 │
│ balance         │       │ amount               │
│ available_bal   │       │ fee                  │
│ reserved_bal    │       │ tax                  │
│ currency        │       │ balance_after        │
│ is_active       │       │ status               │
│ row_version     │       │ transaction_date     │
└─────────────────┘       └──────────────────────┘
                                     │
                                     │
                          ┌──────────▼───────────┐
                          │   journal_entries    │
                          ├──────────────────────┤
                          │ id (PK)              │
                          │ transaction_id (FK)  │
                          │ account_id           │
                          │ account_type         │
                          │ amount               │
                          │ description          │
                          │ entry_date           │
                          └──────────────────────┘

┌──────────────────────┐
│ idempotency_records  │
├──────────────────────┤
│ request_key (PK)     │
│ response_json        │
│ created_at           │
│ expires_at           │
└──────────────────────┘
```

### Indexes

**accounts:**
- `PK: id`
- `UNIQUE: account_number`
- `INDEX: user_id` (for GetAccountsByUser)
- `INDEX: created_at` (for reporting)

**ledger_transactions:**
- `PK: id`
- `INDEX: account_id` (for GetTransactionHistory)
- `INDEX: transaction_id` (for idempotency)
- `INDEX: (account_id, transaction_date)` (for date range queries)
- `INDEX: (account_id, type)` (for filtering by type)

**journal_entries:**
- `PK: id`
- `INDEX: transaction_id` (for journal lookup)

**idempotency_records:**
- `PK: request_key`
- `INDEX: expires_at` (for cleanup)

### Data Types

**Money amounts:**
- Type: `DECIMAL(18, 2)`
- Precision: 18 digits total, 2 decimal places
- Rationale: Avoids floating-point errors

**Timestamps:**
- Type: `TIMESTAMP WITH TIME ZONE`
- Always UTC
- Rationale: Consistent across time zones

**Concurrency:**
- `row_version`: `BYTEA` (PostgreSQL) mapped to `byte[]`
- Auto-incremented by EF Core on updates

## Security & Reliability

### Transaction Safety

1. **ACID Compliance**
   - Atomicity: All or nothing commits
   - Consistency: Business rules enforced
   - Isolation: Pessimistic locking
   - Durability: PostgreSQL WAL

2. **Idempotency**
   - Duplicate request detection
   - 24-hour response cache
   - Safe retries

3. **Concurrency Control**
   - Pessimistic locking (SELECT FOR UPDATE)
   - RowVersion for additional safety
   - Prevents race conditions

4. **Validation**
   - Input validation (FluentValidation)
   - Domain validation (business rules)
   - Database constraints (foreign keys, check constraints)

### Error Handling Strategy

```
┌─────────────────────────────────────────────────────┐
│                 Error Type                          │
├─────────────────────────────────────────────────────┤
│ ValidationException      → 400 Bad Request          │
│ InsufficientBalanceException → 400 Bad Request      │
│ DuplicateTransactionException → 409 Conflict        │
│ NotFoundException        → 404 Not Found            │
│ DbUpdateConcurrencyException → 409 Conflict         │
│ Exception (unknown)      → 500 Internal Error       │
└─────────────────────────────────────────────────────┘
```

All errors include:
- `type`: URI reference
- `title`: Human-readable summary
- `status`: HTTP status code
- `detail`: Specific error message
- `extensions`: Additional context (accountId, amounts, etc.)
- `traceId`: For log correlation

### Monitoring & Observability

**Structured Logging (Serilog):**
- JSON format
- Contextual properties (accountId, transactionId, userId)
- File rotation (daily, 30-day retention)
- Console output in development

**Metrics (Prometheus):**
- `ledger_transactions_total`: Total transactions by type and status
- `ledger_api_request_duration_seconds`: API latency histogram
- `ledger_concurrent_operations`: Active operation count
- `ledger_insufficient_balance_total`: Failed payment attempts

**Health Checks:**
- `/health`: Overall system health
- `/health/ready`: Database connectivity
- `/health/live`: Application responsiveness

## Performance Considerations

### Query Optimization

1. **Read Queries Use AsNoTracking()**
   ```csharp
   return await _context.Accounts
       .AsNoTracking()
       .Where(a => a.UserId == userId)
       .ToListAsync();
   ```
   - Faster (no change tracking overhead)
   - Suitable for read-only queries

2. **Pagination for Large Result Sets**
   ```csharp
   var count = await query.CountAsync();
   var items = await query
       .Skip((pageNumber - 1) * pageSize)
       .Take(pageSize)
       .ToListAsync();
   ```

3. **Indexed Queries**
   - All foreign keys indexed
   - Composite indexes for common filters

### Scalability

**Horizontal Scaling:**
- API layer is stateless
- Can run multiple instances behind load balancer
- Idempotency cache can use Redis for distributed cache

**Database Scaling:**
- Read replicas for queries
- Write to primary, read from replicas
- Connection string separation: `WriteDatabase`, `ReadDatabase`

**Current Setup:**
- Single database (both read/write)
- Ready for read replica: just change `ReadDatabase` connection string

### Metrics-Based Optimization

**Low-Cardinality Labels:**
- ✅ `method`, `endpoint` (route template), `status_code`
- ❌ Removed: `account_id` (high cardinality)

**Histogram Buckets:**
- API latency: 10ms, 50ms, 100ms, 250ms, 500ms, 1s, 2.5s, 5s, 10s
- DB latency: 1ms, 5ms, 10ms, 25ms, 50ms, 100ms, 250ms, 500ms, 1s

## Technology Stack

### Backend
- **.NET 8.0**: Latest LTS version
- **ASP.NET Core**: Web API framework
- **Entity Framework Core 8**: ORM
- **MediatR**: CQRS implementation
- **FluentValidation**: Input validation
- **Serilog**: Structured logging

### Database
- **PostgreSQL 16**: Primary database
- **EF Core Migrations**: Schema management

### Monitoring
- **Prometheus**: Metrics collection
- **Serilog**: Application logging

### DevOps
- **Docker**: Containerization
- **Docker Compose**: Local development

## Future Enhancements

### Planned Features

1. **Event Sourcing**
   - Store state changes as events
   - Replay events to rebuild state
   - Temporal queries

2. **Read Models (CQRS)**
   - Separate read models for queries
   - Denormalized views
   - Caching strategies

3. **Distributed Caching (Redis)**
   - Share idempotency cache across instances
   - Session management
   - Rate limiting

4. **Message Queue (RabbitMQ/Kafka)**
   - Async transaction processing
   - Event-driven architecture
   - Webhook notifications

5. **Multi-Tenancy**
   - Tenant isolation
   - Schema per tenant
   - Row-level security

### Performance Improvements

- [ ] Database query optimization
- [ ] Connection pooling tuning
- [ ] Response caching (Redis)
- [ ] Batch operations
- [ ] GraphQL API (alternative to REST)

### Operational Improvements

- [ ] Grafana dashboards
- [ ] Alert rules (Prometheus AlertManager)
- [ ] Distributed tracing (OpenTelemetry)
- [ ] Automated backups
- [ ] Disaster recovery plan

## Conclusion

The Volcanion Ledger Service is designed with production reliability, maintainability, and scalability in mind. Key architectural decisions prioritize:

- **Correctness**: ACID transactions, validation, concurrency control
- **Reliability**: Idempotency, error handling, monitoring
- **Maintainability**: Clean architecture, separation of concerns, testability
- **Performance**: Optimized queries, horizontal scalability, metrics

For more information:
- [Setup Guide](SETUP.md)
- [Quick Start](QUICK_START.md)
- [Contributing](../CONTRIBUTING.md)
