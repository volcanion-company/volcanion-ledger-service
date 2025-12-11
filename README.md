# Volcanion Ledger Service

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-17-336791?logo=postgresql)](https://www.postgresql.org/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Docker](https://img.shields.io/badge/Docker-Ready-2496ED?logo=docker)](https://www.docker.com/)

Production-grade financial ledger service for payment gateways and e-wallet applications. Built with .NET 8, PostgreSQL, and Clean Architecture principles.

## 🚀 Quick Start

### Using Docker (Recommended)

```bash
# Clone and run
git clone https://github.com/volcanion-company/volcanion-ledger-service.git
cd volcanion-ledger-service
docker compose up -d

# Verify health
curl http://localhost:5000/health

# Access Swagger UI
open http://localhost:5000/swagger
```

### Manual Setup

See [Setup Guide](docs/SETUP.md) for detailed instructions.

## 📋 Features

### Core Capabilities

- ✅ **Account Management** - Multi-currency account creation and management
- ✅ **Transaction Processing** - Topup, payment, refund, and manual adjustments
- ✅ **Double-Entry Bookkeeping** - Complete audit trail with journal entries
- ✅ **Idempotency** - Duplicate request detection prevents double-charging
- ✅ **Concurrency Control** - Pessimistic locking prevents race conditions
- ✅ **ACID Transactions** - Atomic operations ensure data consistency
- ✅ **Validation** - Automatic input validation using FluentValidation
- ✅ **Error Handling** - RFC 7807 compliant error responses

### Observability

- 📊 **Prometheus Metrics** - Request latency, transaction counts, error rates
- 📝 **Structured Logging** - JSON logs with correlation IDs (Serilog)
- 🏥 **Health Checks** - Liveness, readiness, and dependency health endpoints
- 📈 **Monitoring** - Built-in metrics endpoint for Prometheus/Grafana

## 🏗️ Architecture

Built with **Clean Architecture** and **CQRS** patterns:

```
API Layer (Controllers, Middleware)
    ↓
Application Layer (CQRS Handlers, Validation)
    ↓
Domain Layer (Entities, Business Rules)
    ↓
Infrastructure Layer (Database, External Services)
```

**Key Design Decisions:**
- **MediatR Pipeline** - Automatic validation and idempotency
- **Pessimistic Locking** - `SELECT FOR UPDATE` for balance operations
- **Single Commit** - One database transaction per business operation
- **Value Objects** - Money, TransactionId for type safety

See [Architecture Guide](docs/ARCHITECTURE.md) for details.

## 📚 Documentation

| Document | Description |
|----------|-------------|
| [Quick Start](docs/QUICK_START.md) | API usage examples and common workflows |
| [Setup Guide](docs/SETUP.md) | Manual installation and configuration |
| [Docker Setup](docs/SETUP_DOCKER.md) | Docker and Docker Compose guide |
| [Architecture](docs/ARCHITECTURE.md) | System design and technical decisions |
| [Contributing](CONTRIBUTING.md) | Development guidelines and standards |

## 🔧 Technology Stack

| Category | Technology |
|----------|------------|
| **Framework** | .NET 8.0, ASP.NET Core |
| **Database** | PostgreSQL 17 |
| **ORM** | Entity Framework Core 8 |
| **Patterns** | CQRS (MediatR), Repository, Unit of Work |
| **Validation** | FluentValidation |
| **Logging** | Serilog (JSON structured logs) |
| **Metrics** | Prometheus |
| **Containerization** | Docker, Docker Compose |

## 📖 API Overview

### Endpoints

#### Accounts

```http
POST   /api/v1/accounts              # Create account
GET    /api/v1/accounts/{id}         # Get account details
GET    /api/v1/accounts/user/{userId} # Get accounts by user
POST   /api/v1/accounts/{id}/lock    # Lock account
POST   /api/v1/accounts/{id}/unlock  # Unlock account
GET    /api/v1/accounts/{id}/transactions # Get transaction history
```

#### Transactions

```http
POST   /api/v1/transactions/topup     # Add money
POST   /api/v1/transactions/payment   # Process payment
POST   /api/v1/transactions/refund    # Process refund
POST   /api/v1/transactions/adjustment # Manual adjustment
GET    /api/v1/transactions/{id}      # Get transaction details
```

#### Health & Monitoring

```http
GET    /health       # Overall health
GET    /health/ready # Readiness check
GET    /health/live  # Liveness check
GET    /metrics      # Prometheus metrics
```

### Example: Create Account and Topup

```bash
# Create account
curl -X POST http://localhost:5000/api/v1/accounts \
  -H "Content-Type: application/json" \
  -d '{
    "userId": "user123",
    "currency": "VND"
  }'

# Response
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "accountNumber": "ACC20251211123456789",
  "balance": 0,
  "currency": "VND"
}

# Topup account
curl -X POST http://localhost:5000/api/v1/transactions/topup \
  -H "Content-Type: application/json" \
  -d '{
    "accountId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "amount": 100000,
    "transactionId": "TXN001",
    "description": "Initial deposit"
  }'

# Response
{
  "id": "tx-guid",
  "type": "Topup",
  "amount": 100000,
  "balanceAfter": 100000,
  "status": "Completed"
}
```

See [Quick Start Guide](docs/QUICK_START.md) for more examples.

## 🧪 Testing

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test tests/Volcanion.LedgerService.Domain.Tests
```

## 🚀 Deployment

### Docker Deployment

```bash
# Production build
docker compose -f docker-compose.yml -f docker-compose.prod.yml up -d

# Environment variables
docker compose --env-file .env.production up -d
```

### Manual Deployment

```bash
# Publish
dotnet publish -c Release -o ./publish

# Run
cd publish
dotnet Volcanion.LedgerService.API.dll
```

### Environment Configuration

**Development** (`appsettings.Development.json`):
```json
{
  "ConnectionStrings": {
    "WriteDatabase": "Host=localhost;Port=5432;Database=ledger_db_dev;..."
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:3000"]
  }
}
```

**Production** (`appsettings.json`):
```json
{
  "ConnectionStrings": {
    "WriteDatabase": "Host=prod-db;Port=5432;Database=ledger_db;..."
  },
  "Cors": {
    "AllowedOrigins": []
  }
}
```

## 📊 Monitoring

### Prometheus Metrics

```bash
# View metrics
curl http://localhost:5000/metrics

# Key metrics
ledger_transactions_total{type="Topup",status="Completed"}
ledger_api_request_duration_seconds{method="POST",endpoint="/api/v1/transactions/topup"}
ledger_insufficient_balance_total
```

### Health Checks

```bash
# Check health
curl http://localhost:5000/health

# Response
{
  "status": "Healthy",
  "checks": [
    {
      "name": "postgres-write",
      "status": "Healthy",
      "duration": 12.5
    }
  ],
  "totalDuration": 15.2
}
```

### Logs

```bash
# Application logs
cat logs/ledger-service-2025-12-11.log

# Docker logs
docker compose logs -f ledger-api

# Structured log format (JSON)
{
  "Timestamp": "2025-12-11T10:30:00.123Z",
  "Level": "Information",
  "Message": "Payment processed successfully",
  "Properties": {
    "AccountId": "...",
    "TransactionId": "TXN001",
    "Amount": 100000
  }
}
```

## 🔒 Security Features

- **Input Validation** - FluentValidation for all inputs
- **Concurrency Control** - Pessimistic locking prevents race conditions
- **Idempotency** - Duplicate request prevention
- **Audit Trail** - Complete transaction history with journal entries
- **Error Handling** - No sensitive data in error messages
- **HTTPS** - Enforced in production
- **CORS** - Configurable per environment

## 🤝 Contributing

We welcome contributions! Please see [CONTRIBUTING.md](CONTRIBUTING.md) for:

- Development setup
- Coding standards
- Pull request process
- Testing guidelines

### Development Workflow

```bash
# Fork and clone
git clone https://github.com/your-username/volcanion-ledger-service.git
cd volcanion-ledger-service

# Create feature branch
git checkout -b feature/your-feature

# Make changes and test
dotnet build
dotnet test

# Submit pull request
```

## 📄 License

This project is licensed under the MIT License - see [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

Built with:
- [.NET](https://dotnet.microsoft.com/) - Cross-platform framework
- [PostgreSQL](https://www.postgresql.org/) - Powerful open-source database
- [MediatR](https://github.com/jbogard/MediatR) - CQRS implementation
- [FluentValidation](https://fluentvalidation.net/) - Validation library
- [Serilog](https://serilog.net/) - Structured logging
- [Prometheus](https://prometheus.io/) - Metrics and monitoring

## 📞 Support

- **Documentation**: [docs/](docs/)
- **Issues**: [GitHub Issues](https://github.com/volcanion-company/volcanion-ledger-service/issues)
- **Email**: support@volcanion.com

## 🗺️ Roadmap

- [ ] Event Sourcing support
- [ ] Read models (CQRS read side)
- [ ] Distributed caching (Redis)
- [ ] Message queue integration (RabbitMQ/Kafka)
- [ ] Multi-tenancy support
- [ ] GraphQL API
- [ ] Grafana dashboards
- [ ] OpenTelemetry distributed tracing

---

**Built with ❤️ by Volcanion Company**

© 2025 Volcanion Company. All rights reserved.
