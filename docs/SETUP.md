# Setup Guide - Volcanion Ledger Service

This guide covers the setup process for the Volcanion Ledger Service without Docker. For Docker setup, see [SETUP_DOCKER.md](SETUP_DOCKER.md).

## Table of Contents

- [Prerequisites](#prerequisites)
- [Environment Setup](#environment-setup)
- [Database Setup](#database-setup)
- [Application Configuration](#application-configuration)
- [Running the Application](#running-the-application)
- [Verification](#verification)
- [Troubleshooting](#troubleshooting)

## Prerequisites

### Required Software

1. **.NET 8.0 SDK**
   - Download: https://dotnet.microsoft.com/download/dotnet/8.0
   - Verify installation:
     ```powershell
     dotnet --version
     # Should output: 8.0.x or higher
     ```

2. **PostgreSQL 15+**
   - Download: https://www.postgresql.org/download/
   - Recommended: PostgreSQL 15 or 16
   - Alternative: Use Docker for PostgreSQL only

3. **Git**
   - Download: https://git-scm.com/downloads

4. **IDE (Optional but recommended)**
   - Visual Studio 2022 (Community or higher)
   - Visual Studio Code with C# extension
   - JetBrains Rider

## Environment Setup

### 1. Clone the Repository

```powershell
git clone https://github.com/volcanion-company/volcanion-ledger-service.git
cd volcanion-ledger-service
```

### 2. Restore Dependencies

```powershell
dotnet restore
```

### 3. Build the Solution

```powershell
dotnet build
```

## Database Setup

### Option 1: Local PostgreSQL Installation

#### Step 1: Install PostgreSQL

1. Download PostgreSQL installer
2. Run installer with these settings:
   - Port: `5432` (default)
   - Password: Set a secure password
   - Locale: English, United States

#### Step 2: Create Database

```powershell
# Connect to PostgreSQL (use your password)
psql -U postgres

# Create database and user
CREATE DATABASE ledger_db;
CREATE DATABASE ledger_db_dev;

# Create user (optional, can use postgres user)
CREATE USER ledger_user WITH ENCRYPTED PASSWORD 'your_secure_password';

# Grant privileges
GRANT ALL PRIVILEGES ON DATABASE ledger_db TO ledger_user;
GRANT ALL PRIVILEGES ON DATABASE ledger_db_dev TO ledger_user;

# Exit psql
\q
```

#### Step 3: Verify Connection

```powershell
psql -U postgres -d ledger_db -c "SELECT version();"
```

### Option 2: PostgreSQL via Docker

If you prefer not to install PostgreSQL locally:

```powershell
# Run PostgreSQL container
docker run --name ledger-postgres `
  -e POSTGRES_PASSWORD=postgres `
  -e POSTGRES_DB=ledger_db `
  -p 5432:5432 `
  -d postgres:16-alpine

# Verify container is running
docker ps

# Create development database
docker exec -it ledger-postgres psql -U postgres -c "CREATE DATABASE ledger_db_dev;"
```

## Application Configuration

### 1. Configure Connection Strings

Edit `src/Volcanion.LedgerService.API/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "WriteDatabase": "Host=localhost;Port=5432;Database=ledger_db_dev;Username=postgres;Password=your_password;Include Error Detail=true",
    "ReadDatabase": "Host=localhost;Port=5432;Database=ledger_db_dev;Username=postgres;Password=your_password;Include Error Detail=true"
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:3000", "http://localhost:5173"]
  }
}
```

For production (`appsettings.json`):

```json
{
  "ConnectionStrings": {
    "WriteDatabase": "Host=your-db-host;Port=5432;Database=ledger_db;Username=ledger_user;Password=secure_password;Include Error Detail=false",
    "ReadDatabase": "Host=your-db-host;Port=5432;Database=ledger_db;Username=ledger_user;Password=secure_password;Include Error Detail=false"
  },
  "Cors": {
    "AllowedOrigins": []
  }
}
```

### 2. Apply Database Migrations

The application auto-migrates on startup in Development mode. For manual migration:

```powershell
# Navigate to API project
cd src/Volcanion.LedgerService.API

# Apply migrations
dotnet ef database update --project ../Volcanion.LedgerService.Infrastructure
```

### 3. Configure Logging

Logs are configured in `appsettings.json` and written to:
- Console (all environments)
- File: `logs/ledger-service-YYYY-MM-DD.log`

## Running the Application

### Development Mode

```powershell
# From solution root
cd src/Volcanion.LedgerService.API

# Run with hot reload
dotnet watch run

# Or run without hot reload
dotnet run
```

The application will start on:
- HTTPS: https://localhost:7001
- HTTP: http://localhost:5000

### Production Mode

```powershell
# Build release
dotnet publish -c Release -o ./publish

# Run published app
cd publish
dotnet Volcanion.LedgerService.API.dll
```

## Verification

### 1. Check Health Endpoints

```powershell
# Health check
curl https://localhost:7001/health

# Readiness check
curl https://localhost:7001/health/ready

# Liveness check
curl https://localhost:7001/health/live
```

Expected response:
```json
{
  "status": "Healthy",
  "checks": [
    {
      "name": "postgres-write",
      "status": "Healthy"
    },
    {
      "name": "postgres-read",
      "status": "Healthy"
    }
  ]
}
```

### 2. Access Swagger UI

Open browser: https://localhost:7001/swagger

### 3. Test API Endpoints

#### Create an Account

```powershell
curl -X POST https://localhost:7001/api/v1/accounts `
  -H "Content-Type: application/json" `
  -d '{
    "userId": "user123",
    "currency": "VND"
  }'
```

#### Topup Account

```powershell
curl -X POST https://localhost:7001/api/v1/transactions/topup `
  -H "Content-Type: application/json" `
  -d '{
    "accountId": "YOUR_ACCOUNT_ID",
    "amount": 100000,
    "transactionId": "TXN123456",
    "description": "Test topup"
  }'
```

### 4. Check Prometheus Metrics

```powershell
curl https://localhost:7001/metrics
```

### 5. View Logs

```powershell
# View live logs (console)
# Logs appear in terminal

# View file logs
Get-Content logs/ledger-service-*.log -Tail 50 -Wait
```

## Running Tests

```powershell
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test tests/Volcanion.LedgerService.Domain.Tests
```

## Troubleshooting

### Database Connection Issues

**Problem**: Cannot connect to PostgreSQL

**Solutions**:
1. Check PostgreSQL is running:
   ```powershell
   # Windows
   Get-Service postgresql*
   
   # Or check process
   Get-Process postgres
   ```

2. Verify connection string:
   ```powershell
   psql -h localhost -U postgres -d ledger_db
   ```

3. Check firewall settings (port 5432)

4. Verify password in connection string

### Migration Issues

**Problem**: Migration fails or database not created

**Solutions**:
1. Drop and recreate database:
   ```sql
   DROP DATABASE IF EXISTS ledger_db_dev;
   CREATE DATABASE ledger_db_dev;
   ```

2. Clear EF Core migrations cache:
   ```powershell
   Remove-Item -Recurse -Force src/Volcanion.LedgerService.Infrastructure/bin
   Remove-Item -Recurse -Force src/Volcanion.LedgerService.Infrastructure/obj
   ```

3. Recreate migrations:
   ```powershell
   cd src/Volcanion.LedgerService.Infrastructure
   dotnet ef migrations remove
   dotnet ef migrations add InitialCreate
   ```

### Port Already in Use

**Problem**: Port 5000 or 7001 already in use

**Solutions**:
1. Find and kill process:
   ```powershell
   # Find process on port
   netstat -ano | findstr :7001
   
   # Kill process (replace PID)
   taskkill /PID <PID> /F
   ```

2. Change port in `Properties/launchSettings.json`:
   ```json
   "applicationUrl": "https://localhost:7002;http://localhost:5001"
   ```

### Swagger Not Loading

**Problem**: Swagger UI shows error or doesn't load

**Solutions**:
1. Ensure running in Development mode:
   ```powershell
   $env:ASPNETCORE_ENVIRONMENT="Development"
   dotnet run
   ```

2. Clear browser cache
3. Check API is running: curl https://localhost:7001/health

### Validation Errors

**Problem**: Requests return 400 Bad Request

**Solutions**:
1. Check request body format (JSON)
2. Verify all required fields are provided
3. Check data types (decimal for amounts, GUID for IDs)
4. Review validation errors in response body

### Performance Issues

**Problem**: Slow response times

**Solutions**:
1. Check database indexes:
   ```sql
   SELECT * FROM pg_indexes WHERE tablename = 'accounts';
   ```

2. Enable query logging:
   ```json
   "Logging": {
     "LogLevel": {
       "Microsoft.EntityFrameworkCore.Database.Command": "Information"
     }
   }
   ```

3. Monitor metrics: https://localhost:7001/metrics

## Next Steps

- See [QUICK_START.md](QUICK_START.md) for API usage examples
- See [ARCHITECTURE.md](ARCHITECTURE.md) for system design
- Review [API documentation](https://localhost:7001/swagger)

## Getting Help

- Check [GitHub Issues](https://github.com/volcanion-company/volcanion-ledger-service/issues)
- Review logs in `logs/` directory
- Contact: support@volcanion.com
