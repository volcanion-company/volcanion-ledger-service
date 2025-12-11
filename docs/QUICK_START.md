# Quick Start Guide - Volcanion Ledger Service

Get started with the Volcanion Ledger Service in minutes. This guide shows you how to perform common operations.

## Table of Contents

- [First Time Setup](#first-time-setup)
- [Core Concepts](#core-concepts)
- [Common Operations](#common-operations)
- [Example Workflows](#example-workflows)
- [API Reference](#api-reference)

## First Time Setup

### Option 1: Docker (Recommended)

```powershell
# Clone and start
git clone https://github.com/volcanion-company/volcanion-ledger-service.git
cd volcanion-ledger-service
docker compose up -d

# Verify
curl http://localhost:5000/health
```

### Option 2: Manual Setup

See [SETUP.md](SETUP.md) for detailed instructions.

## Core Concepts

### Account
A ledger account representing a user's balance in a specific currency.

**Properties:**
- `accountNumber`: Unique identifier (e.g., ACC20251211123456789)
- `userId`: User identifier from your system
- `balance`: Current total balance
- `availableBalance`: Balance available for spending
- `currency`: Three-letter currency code (VND, USD, etc.)

### Transaction Types

| Type | Description | Balance Impact |
|------|-------------|----------------|
| **Topup** | Add money to account | Increases balance |
| **Payment** | Deduct money for purchase | Decreases balance |
| **Refund** | Return money from previous payment | Increases balance |
| **Adjustment** | Manual balance correction | Increases balance |

### Transaction ID (Idempotency)
Each transaction requires a unique `transactionId`. Sending the same `transactionId` twice returns the original result (prevents duplicate charges).

## Common Operations

### 1. Create an Account

```powershell
curl -X POST http://localhost:5000/api/v1/accounts `
  -H "Content-Type: application/json" `
  -d '{
    "userId": "user123",
    "currency": "VND"
  }'
```

**Response:**
```json
{
  "id": "a1b2c3d4-e5f6-4789-a012-3456789abcde",
  "accountNumber": "ACC20251211123456789",
  "userId": "user123",
  "balance": 0,
  "availableBalance": 0,
  "reservedBalance": 0,
  "currency": "VND",
  "isActive": true,
  "createdAt": "2025-12-11T10:30:00Z"
}
```

💡 **Save the `id`** - you'll need it for transactions!

### 2. Add Money (Topup)

```powershell
curl -X POST http://localhost:5000/api/v1/transactions/topup `
  -H "Content-Type: application/json" `
  -d '{
    "accountId": "a1b2c3d4-e5f6-4789-a012-3456789abcde",
    "amount": 100000,
    "transactionId": "TXN001",
    "description": "Initial deposit"
  }'
```

**Response:**
```json
{
  "id": "tx-guid-here",
  "accountId": "a1b2c3d4-e5f6-4789-a012-3456789abcde",
  "transactionId": "TXN001",
  "type": "Topup",
  "status": "Completed",
  "amount": 100000,
  "balanceAfter": 100000,
  "transactionDate": "2025-12-11T10:31:00Z"
}
```

✅ Account balance is now **100,000 VND**

### 3. Process a Payment

```powershell
curl -X POST http://localhost:5000/api/v1/transactions/payment `
  -H "Content-Type: application/json" `
  -d '{
    "accountId": "a1b2c3d4-e5f6-4789-a012-3456789abcde",
    "amount": 50000,
    "fee": 1000,
    "tax": 500,
    "transactionId": "TXN002",
    "merchantId": "MERCHANT001",
    "description": "Purchase from Store A"
  }'
```

**Response:**
```json
{
  "id": "tx-guid-here",
  "accountId": "a1b2c3d4-e5f6-4789-a012-3456789abcde",
  "transactionId": "TXN002",
  "type": "Payment",
  "status": "Completed",
  "amount": 50000,
  "fee": 1000,
  "tax": 500,
  "balanceAfter": 48500,
  "merchantId": "MERCHANT001",
  "transactionDate": "2025-12-11T10:32:00Z"
}
```

💸 **Total deducted**: 50,000 + 1,000 + 500 = **51,500 VND**

### 4. Get Account Details

```powershell
curl http://localhost:5000/api/v1/accounts/a1b2c3d4-e5f6-4789-a012-3456789abcde
```

**Response:**
```json
{
  "id": "a1b2c3d4-e5f6-4789-a012-3456789abcde",
  "accountNumber": "ACC20251211123456789",
  "userId": "user123",
  "balance": 48500,
  "availableBalance": 48500,
  "reservedBalance": 0,
  "currency": "VND",
  "isActive": true
}
```

### 5. Get Transaction History

```powershell
curl "http://localhost:5000/api/v1/accounts/a1b2c3d4-e5f6-4789-a012-3456789abcde/transactions?pageNumber=1&pageSize=10"
```

**Response:**
```json
{
  "items": [
    {
      "id": "tx2-guid",
      "type": "Payment",
      "amount": 50000,
      "balanceAfter": 48500,
      "transactionDate": "2025-12-11T10:32:00Z"
    },
    {
      "id": "tx1-guid",
      "type": "Topup",
      "amount": 100000,
      "balanceAfter": 100000,
      "transactionDate": "2025-12-11T10:31:00Z"
    }
  ],
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 1,
  "totalCount": 2
}
```

## Example Workflows

### Workflow 1: E-commerce Purchase

```powershell
# Step 1: Customer tops up wallet
curl -X POST http://localhost:5000/api/v1/transactions/topup `
  -H "Content-Type: application/json" `
  -d '{
    "accountId": "user-account-id",
    "amount": 500000,
    "transactionId": "TOPUP_2025_001"
  }'

# Step 2: Customer makes purchase
curl -X POST http://localhost:5000/api/v1/transactions/payment `
  -H "Content-Type: application/json" `
  -d '{
    "accountId": "user-account-id",
    "amount": 250000,
    "fee": 5000,
    "tax": 2500,
    "transactionId": "ORDER_2025_001",
    "merchantId": "SHOP_123"
  }'

# Step 3: Customer requests refund
curl -X POST http://localhost:5000/api/v1/transactions/refund `
  -H "Content-Type: application/json" `
  -d '{
    "accountId": "user-account-id",
    "amount": 250000,
    "transactionId": "REFUND_2025_001",
    "originalTransactionId": "ORDER_2025_001"
  }'
```

### Workflow 2: Insufficient Balance Handling

```powershell
# Attempt payment with insufficient balance
curl -X POST http://localhost:5000/api/v1/transactions/payment `
  -H "Content-Type: application/json" `
  -d '{
    "accountId": "user-account-id",
    "amount": 1000000,
    "fee": 0,
    "tax": 0,
    "transactionId": "TXN_INSUFFICIENT"
  }'
```

**Error Response (400):**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Insufficient Balance",
  "status": 400,
  "detail": "Account does not have sufficient balance",
  "extensions": {
    "accountId": "user-account-id",
    "required": 1000000,
    "available": 500000,
    "traceId": "00-abc123..."
  }
}
```

### Workflow 3: Idempotency Test

```powershell
# Send same transaction twice
curl -X POST http://localhost:5000/api/v1/transactions/topup `
  -H "Content-Type: application/json" `
  -d '{
    "accountId": "user-account-id",
    "amount": 100000,
    "transactionId": "DUPLICATE_TEST"
  }'

# Send again (will return cached result, no double-charge)
curl -X POST http://localhost:5000/api/v1/transactions/topup `
  -H "Content-Type: application/json" `
  -d '{
    "accountId": "user-account-id",
    "amount": 100000,
    "transactionId": "DUPLICATE_TEST"
  }'
```

✅ Balance only increases once, second request returns original transaction.

## API Reference

### Base URL

- Development: `http://localhost:5000`
- Production: `https://your-domain.com`

### Authentication

Currently no authentication (add your own middleware).

### Endpoints

#### Accounts

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/v1/accounts` | Create account |
| GET | `/api/v1/accounts/{id}` | Get account details |
| GET | `/api/v1/accounts/user/{userId}` | Get accounts by user |
| POST | `/api/v1/accounts/{id}/lock` | Lock account |
| POST | `/api/v1/accounts/{id}/unlock` | Unlock account |
| GET | `/api/v1/accounts/{id}/transactions` | Get transaction history |

#### Transactions

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/v1/transactions/topup` | Add money |
| POST | `/api/v1/transactions/payment` | Process payment |
| POST | `/api/v1/transactions/refund` | Process refund |
| POST | `/api/v1/transactions/adjustment` | Manual adjustment |
| GET | `/api/v1/transactions/{id}` | Get transaction details |

#### Health & Monitoring

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/health` | Health check |
| GET | `/health/ready` | Readiness check |
| GET | `/health/live` | Liveness check |
| GET | `/metrics` | Prometheus metrics |

### Request Examples with PowerShell

#### Create Account
```powershell
$body = @{
    userId = "user123"
    currency = "VND"
} | ConvertTo-Json

Invoke-RestMethod -Method Post -Uri "http://localhost:5000/api/v1/accounts" `
    -ContentType "application/json" -Body $body
```

#### Topup
```powershell
$body = @{
    accountId = "your-account-id"
    amount = 100000
    transactionId = "TXN001"
    description = "Deposit"
} | ConvertTo-Json

Invoke-RestMethod -Method Post -Uri "http://localhost:5000/api/v1/transactions/topup" `
    -ContentType "application/json" -Body $body
```

#### Get Transaction History
```powershell
Invoke-RestMethod -Method Get `
    -Uri "http://localhost:5000/api/v1/accounts/your-account-id/transactions?pageNumber=1&pageSize=10"
```

### Error Handling

All errors follow [RFC 7807 Problem Details](https://tools.ietf.org/html/rfc7807):

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Validation Error",
  "status": 400,
  "errors": {
    "Amount": ["Amount must be greater than 0"]
  },
  "traceId": "00-abc123..."
}
```

**Common Error Codes:**
- `400` - Bad Request (validation errors)
- `404` - Not Found (account/transaction not found)
- `409` - Conflict (account locked, negative balance)
- `500` - Internal Server Error

## Testing with Swagger

1. Open Swagger UI: http://localhost:5000/swagger
2. Click on an endpoint (e.g., `POST /api/v1/accounts`)
3. Click **Try it out**
4. Fill in the request body
5. Click **Execute**
6. View response

## Monitoring & Observability

### View Metrics

```powershell
# Prometheus metrics
curl http://localhost:5000/metrics

# Key metrics:
# - ledger_transactions_total
# - ledger_api_request_duration_seconds
# - ledger_concurrent_operations
```

### Check Logs

```powershell
# Application logs (file)
Get-Content logs/ledger-service-*.log -Tail 50 -Wait

# Docker logs
docker compose logs -f ledger-api
```

### Health Checks

```powershell
# Overall health
curl http://localhost:5000/health | ConvertFrom-Json | Format-List

# Database connection
curl http://localhost:5000/health | ConvertFrom-Json | 
    Select-Object -ExpandProperty checks | 
    Where-Object { $_.name -like "*postgres*" }
```

## Best Practices

### Transaction IDs

✅ **Good:**
- Use UUIDs: `550e8400-e29b-41d4-a716-446655440000`
- Use sequential IDs: `TXN_2025_001`, `ORDER_12345`
- Include timestamp: `TOPUP_20251211_123456`

❌ **Bad:**
- Random numbers: `12345`
- Reusing IDs across different transactions
- Non-unique identifiers

### Amount Handling

- Always use **integers** (smallest unit)
- For VND: 100000 = 100,000 VND
- For USD: 10050 = $100.50 (cents)

### Error Handling

```powershell
# Check response status
try {
    $response = Invoke-RestMethod -Method Post -Uri $url -Body $body
    Write-Host "Success: $($response.id)"
} catch {
    $error = $_.ErrorDetails.Message | ConvertFrom-Json
    Write-Error "Failed: $($error.title) - $($error.detail)"
}
```

## Next Steps

- **Architecture**: See [ARCHITECTURE.md](ARCHITECTURE.md) for system design
- **Setup**: See [SETUP.md](SETUP.md) or [SETUP_DOCKER.md](SETUP_DOCKER.md)
- **Contributing**: See [CONTRIBUTING.md](../CONTRIBUTING.md)

## Support

- **Issues**: https://github.com/volcanion-company/volcanion-ledger-service/issues
- **Email**: support@volcanion.com
- **Documentation**: http://localhost:5000/swagger
