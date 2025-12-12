# Postman Collection Guide

## Overview
This collection contains all API endpoints for the Volcanion Ledger Service, including examples and a complete workflow demonstration.

## Files
- `Volcanion-Ledger-Service.postman_collection.json` - Main collection with all endpoints
- `Volcanion-Ledger-Service.postman_environment.json` - Environment variables for local development

## How to Import

1. **Import Collection**
   - Open Postman
   - Click "Import" button
   - Select `Volcanion-Ledger-Service.postman_collection.json`
   - Click "Import"

2. **Import Environment**
   - Click "Import" button
   - Select `Volcanion-Ledger-Service.postman_environment.json`
   - Click "Import"
   - Select "Ledger Service Environments" from the environment dropdown

## Collection Structure

### 1. Accounts
- **POST** `/api/v1/accounts` - Create a new account
- **GET** `/api/v1/accounts/{id}` - Get account by ID
- **GET** `/api/v1/accounts/user/{userId}` - Get account by user ID

### 2. Transactions
- **POST** `/api/v1/transactions/topup` - Add funds to account
- **POST** `/api/v1/transactions/payment` - Process payment (deduct funds)
- **POST** `/api/v1/transactions/refund` - Refund a previous transaction
- **POST** `/api/v1/transactions/adjustment` - Manual balance adjustment
- **GET** `/api/v1/transactions/history/{accountId}` - Get transaction history (with filters)

### 3. Health Checks
- **GET** `/health` - Overall health status
- **GET** `/health/ready` - Readiness probe
- **GET** `/health/live` - Liveness probe

### 4. Metrics
- **GET** `/metrics` - Prometheus metrics

### 5. Complete Workflow Example
A pre-configured workflow that demonstrates:
1. Create account
2. Topup (add 500,000 VND)
3. Payment (deduct 103,000 VND including fees and tax)
4. Check balance
5. View transaction history
6. Process refund
7. Apply adjustment
8. Final balance check

## Environment Variables

### Default Variables
```
baseUrl: http://localhost:5000
userId: user-001
```

### Auto-populated Variables
These are set automatically by test scripts:
- `accountId` - Set after creating an account
- `accountNumber` - Set after creating an account
- `lastPaymentTransactionId` - Set after processing a payment

## Quick Start

### Option 1: Run Complete Workflow
1. Select "Complete Workflow Example" folder
2. Click "Run" button to execute all requests in sequence
3. Variables will be automatically populated between requests

### Option 2: Manual Testing
1. Create an account first
2. Copy the returned `id` and set it as `accountId` environment variable
3. Run other requests using the account ID

## Request Examples

### Create Account
```json
POST /api/v1/accounts
{
  "userId": "user-001",
  "currency": "VND"
}
```

### Topup
```json
POST /api/v1/transactions/topup
{
  "accountId": "{{accountId}}",
  "amount": 100000,
  "transactionId": "TOPUP-1234567890",
  "description": "Bank transfer topup"
}
```

### Process Payment
```json
POST /api/v1/transactions/payment
{
  "accountId": "{{accountId}}",
  "amount": 50000,
  "fee": 1000,
  "tax": 500,
  "transactionId": "PAYMENT-1234567890",
  "merchantId": "MERCHANT-001",
  "description": "Purchase from online store"
}
```

### Get Transaction History (with filters)
```
GET /api/v1/transactions/history/{{accountId}}?page=1&pageSize=50&transactionType=PAYMENT
```

Query parameters:
- `page` (optional): Page number, default 1
- `pageSize` (optional): Items per page, default 50
- `transactionType` (optional): Filter by type (TOPUP, PAYMENT, REFUND, ADJUSTMENT)
- `startDate` (optional): Filter from date (ISO 8601 format)
- `endDate` (optional): Filter to date (ISO 8601 format)

## Currency Support
Supported currencies (ISO 4217):
- `VND` - Vietnamese Dong
- `USD` - US Dollar
- `EUR` - Euro
- `GBP` - British Pound
- `JPY` - Japanese Yen

## Idempotency
All transaction endpoints support idempotency using the `transactionId` field:
- Same `transactionId` will return the cached response
- Idempotency keys expire after 24 hours
- Use unique transaction IDs for each request (e.g., `TOPUP-{{$timestamp}}`)

## Dynamic Variables
Postman provides dynamic variables you can use:
- `{{$timestamp}}` - Current Unix timestamp
- `{{$randomInt}}` - Random integer
- `{{$guid}}` - Random GUID

Example:
```json
{
  "transactionId": "TOPUP-{{$timestamp}}-{{$randomInt}}"
}
```

## Error Handling
The API returns standard HTTP status codes:
- `200 OK` - Request successful
- `201 Created` - Resource created
- `400 Bad Request` - Validation error or business rule violation
- `404 Not Found` - Resource not found
- `409 Conflict` - Duplicate transaction
- `500 Internal Server Error` - Server error

Error response format:
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Validation Error",
  "status": 400,
  "detail": "One or more validation errors occurred.",
  "instance": "/api/v1/transactions/topup",
  "traceId": "0HMVFE3RQNKJ2:00000001"
}
```

## Testing Tips

### 1. Test Insufficient Balance
Create an account, topup small amount, then try to pay more than balance.

### 2. Test Idempotency
Send the same topup request twice with same `transactionId` - should return same response.

### 3. Test Validation
Try negative amounts, empty fields, invalid currency codes.

### 4. Test Pagination
Create many transactions, then test different page sizes and page numbers.

### 5. Test Date Filters
Create transactions on different dates, then filter by date range.

## Performance Testing
Use Postman Collection Runner or Newman CLI for load testing:
```bash
newman run Volcanion-Ledger-Service.postman_collection.json \
  -e Volcanion-Ledger-Service.postman_environment.json \
  -n 100 \
  --delay-request 100
```

## Monitoring
Monitor the service using:
- `/health` - Overall health with database checks
- `/metrics` - Prometheus metrics for monitoring dashboards

## Support
For issues or questions:
- Check API documentation at `/swagger` (in development mode)
- Review logs in `logs/ledger-service-*.log`
- Contact: support@volcanion.com
