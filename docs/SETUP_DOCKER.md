# Docker Setup Guide - Volcanion Ledger Service

This guide covers the setup process using Docker and Docker Compose. For manual setup without Docker, see [SETUP.md](SETUP.md).

## Table of Contents

- [Prerequisites](#prerequisites)
- [Quick Start](#quick-start)
- [Docker Compose Configuration](#docker-compose-configuration)
- [Running with Docker](#running-with-docker)
- [Accessing Services](#accessing-services)
- [Management Commands](#management-commands)
- [Troubleshooting](#troubleshooting)

## Prerequisites

### Required Software

1. **Docker Desktop**
   - Windows: https://docs.docker.com/desktop/install/windows-install/
   - macOS: https://docs.docker.com/desktop/install/mac-install/
   - Linux: https://docs.docker.com/engine/install/

   Verify installation:
   ```powershell
   docker --version
   # Should output: Docker version 24.x.x or higher
   
   docker compose version
   # Should output: Docker Compose version v2.x.x or higher
   ```

2. **Git** (for cloning repository)
   - Download: https://git-scm.com/downloads

## Quick Start

### 1. Clone Repository

```powershell
git clone https://github.com/volcanion-company/volcanion-ledger-service.git
cd volcanion-ledger-service
```

### 2. Start All Services

```powershell
docker compose up -d
```

This single command will:
- Build the Ledger Service API image
- Start PostgreSQL database
- Start Prometheus for metrics
- Configure networking between services
- Apply database migrations automatically

### 3. Verify Services

```powershell
# Check running containers
docker compose ps

# Check API health
curl http://localhost:5000/health
```

### 4. Access the Application

- **API**: http://localhost:5000
- **Swagger UI**: http://localhost:5000/swagger
- **Prometheus**: http://localhost:9090
- **Metrics**: http://localhost:5000/metrics

That's it! The service is now running.

## Docker Compose Configuration

### Services Overview

The `docker-compose.yml` defines three services:

```yaml
services:
  # PostgreSQL Database
  postgres:
    - Port: 5432
    - Database: ledger_db
    - User: postgres
    - Password: postgres (change in production!)
    - Persistent volume: postgres-data

  # Ledger Service API
  ledger-api:
    - Ports: 5000 (HTTP), 5001 (HTTPS)
    - Depends on: postgres
    - Auto-migration: enabled
    - Environment: Development

  # Prometheus Monitoring
  prometheus:
    - Port: 9090
    - Scrapes metrics from ledger-api every 15s
    - Configuration: ./prometheus.yml
```

### Environment Variables

Default environment variables (defined in `docker-compose.yml`):

```yaml
environment:
  - ASPNETCORE_ENVIRONMENT=Development
  - ConnectionStrings__WriteDatabase=Host=postgres;Port=5432;Database=ledger_db;Username=postgres;Password=postgres
  - ConnectionStrings__ReadDatabase=Host=postgres;Port=5432;Database=ledger_db;Username=postgres;Password=postgres
```

### Volume Mounts

Persistent data volumes:
- `postgres-data`: PostgreSQL data directory
- Application logs: `./logs` (mapped to container `/app/logs`)

## Running with Docker

### Start Services

```powershell
# Start all services in background
docker compose up -d

# Start with logs visible
docker compose up

# Start specific service
docker compose up -d postgres
```

### Stop Services

```powershell
# Stop all services (keeps data)
docker compose down

# Stop and remove volumes (DELETES DATA!)
docker compose down -v

# Stop specific service
docker compose stop ledger-api
```

### Rebuild Images

```powershell
# Rebuild all images
docker compose build

# Rebuild and restart
docker compose up -d --build

# Rebuild specific service
docker compose build ledger-api
```

### View Logs

```powershell
# All services
docker compose logs -f

# Specific service
docker compose logs -f ledger-api

# Last 100 lines
docker compose logs --tail=100 ledger-api
```

## Accessing Services

### API Endpoints

#### Health Checks

```powershell
# Overall health
curl http://localhost:5000/health

# Response:
{
  "status": "Healthy",
  "checks": [
    {
      "name": "postgres-write",
      "status": "Healthy"
    }
  ]
}

# Readiness
curl http://localhost:5000/health/ready

# Liveness
curl http://localhost:5000/health/live
```

#### Swagger Documentation

Open browser: http://localhost:5000/swagger

Interactive API documentation with:
- All endpoints documented
- Request/response schemas
- Try-it-out functionality

#### Prometheus Metrics

```powershell
# Raw metrics
curl http://localhost:5000/metrics

# Prometheus UI
# Open browser: http://localhost:9090
```

### Database Access

#### Using psql CLI

```powershell
# Connect to database
docker compose exec postgres psql -U postgres -d ledger_db

# Run SQL query
docker compose exec postgres psql -U postgres -d ledger_db -c "SELECT COUNT(*) FROM accounts;"
```

#### Using Database Client

Connection details:
- **Host**: localhost
- **Port**: 5432
- **Database**: ledger_db
- **Username**: postgres
- **Password**: postgres

### Example API Calls

#### 1. Create Account

```powershell
curl -X POST http://localhost:5000/api/v1/accounts `
  -H "Content-Type: application/json" `
  -d '{
    "userId": "user123",
    "currency": "VND"
  }'
```

Response:
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "accountNumber": "ACC20251211123456789",
  "userId": "user123",
  "balance": 0,
  "currency": "VND",
  "isActive": true
}
```

#### 2. Topup Account

```powershell
curl -X POST http://localhost:5000/api/v1/transactions/topup `
  -H "Content-Type: application/json" `
  -d '{
    "accountId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "amount": 100000,
    "transactionId": "TXN001",
    "description": "Initial deposit"
  }'
```

#### 3. Process Payment

```powershell
curl -X POST http://localhost:5000/api/v1/transactions/payment `
  -H "Content-Type: application/json" `
  -d '{
    "accountId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "amount": 50000,
    "fee": 1000,
    "tax": 500,
    "transactionId": "TXN002",
    "merchantId": "MERCHANT001",
    "description": "Payment to merchant"
  }'
```

#### 4. Get Account Details

```powershell
curl http://localhost:5000/api/v1/accounts/3fa85f64-5717-4562-b3fc-2c963f66afa6
```

#### 5. Get Transaction History

```powershell
curl "http://localhost:5000/api/v1/accounts/3fa85f64-5717-4562-b3fc-2c963f66afa6/transactions?pageNumber=1&pageSize=10"
```

## Management Commands

### Container Management

```powershell
# List running containers
docker compose ps

# View resource usage
docker compose stats

# Restart service
docker compose restart ledger-api

# Execute command in container
docker compose exec ledger-api bash
```

### Database Management

```powershell
# Backup database
docker compose exec postgres pg_dump -U postgres ledger_db > backup.sql

# Restore database
docker compose exec -T postgres psql -U postgres ledger_db < backup.sql

# Reset database (DELETES ALL DATA!)
docker compose down -v
docker compose up -d
```

### Log Management

```powershell
# View application logs
Get-Content logs/ledger-service-*.log -Tail 50 -Wait

# Clear logs
Remove-Item logs/*.log

# Export logs
docker compose logs ledger-api > ledger-api-logs.txt
```

### Monitoring

```powershell
# View Prometheus targets
# Open: http://localhost:9090/targets

# Query metrics in Prometheus
# Example queries:
# - ledger_transactions_total
# - ledger_api_request_duration_seconds
# - process_cpu_seconds_total
```

## Configuration

### Custom Environment Variables

Create `.env` file in project root:

```env
# Database
POSTGRES_PASSWORD=your_secure_password
POSTGRES_DB=ledger_db

# API
ASPNETCORE_ENVIRONMENT=Production
API_PORT=5000
```

Update `docker-compose.yml` to use `.env`:

```yaml
services:
  postgres:
    environment:
      - POSTGRES_PASSWORD=${POSTGRES_PASSWORD}
      - POSTGRES_DB=${POSTGRES_DB}
  
  ledger-api:
    ports:
      - "${API_PORT}:8080"
```

### Custom Prometheus Configuration

Edit `prometheus.yml`:

```yaml
scrape_configs:
  - job_name: 'ledger-service'
    scrape_interval: 15s
    static_configs:
      - targets: ['ledger-api:8080']
    metrics_path: '/metrics'
```

## Troubleshooting

### Service Won't Start

**Problem**: Container exits immediately

**Solutions**:

1. Check logs:
   ```powershell
   docker compose logs ledger-api
   ```

2. Verify database is ready:
   ```powershell
   docker compose ps postgres
   ```

3. Check port conflicts:
   ```powershell
   netstat -ano | findstr :5000
   ```

4. Rebuild image:
   ```powershell
   docker compose build --no-cache ledger-api
   docker compose up -d
   ```

### Database Connection Failed

**Problem**: API cannot connect to PostgreSQL

**Solutions**:

1. Verify postgres is running:
   ```powershell
   docker compose ps postgres
   ```

2. Check connection string:
   ```powershell
   docker compose exec ledger-api printenv | findstr ConnectionStrings
   ```

3. Test database connection:
   ```powershell
   docker compose exec postgres psql -U postgres -c "SELECT 1"
   ```

4. Restart services in order:
   ```powershell
   docker compose down
   docker compose up -d postgres
   # Wait 10 seconds
   docker compose up -d ledger-api
   ```

### Migration Issues

**Problem**: Database tables not created

**Solutions**:

1. Check migration logs:
   ```powershell
   docker compose logs ledger-api | Select-String -Pattern "migration"
   ```

2. Manually run migrations:
   ```powershell
   docker compose exec ledger-api dotnet ef database update --project /app
   ```

3. Reset database:
   ```powershell
   docker compose down -v
   docker compose up -d
   ```

### Port Already in Use

**Problem**: Cannot bind to port 5000

**Solutions**:

1. Find process using port:
   ```powershell
   netstat -ano | findstr :5000
   ```

2. Change port in `docker-compose.yml`:
   ```yaml
   ledger-api:
     ports:
       - "5050:8080"  # Changed from 5000 to 5050
   ```

3. Stop conflicting service:
   ```powershell
   docker compose -f other-project/docker-compose.yml down
   ```

### Out of Memory

**Problem**: Container crashes with OOM

**Solutions**:

1. Increase Docker memory limit (Docker Desktop → Settings → Resources)

2. Add memory limits to `docker-compose.yml`:
   ```yaml
   ledger-api:
     deploy:
       resources:
         limits:
           memory: 1G
         reservations:
           memory: 512M
   ```

### Cannot Access Swagger

**Problem**: http://localhost:5000/swagger returns 404

**Solutions**:

1. Verify environment is Development:
   ```powershell
   docker compose exec ledger-api printenv ASPNETCORE_ENVIRONMENT
   ```

2. Check if API is running:
   ```powershell
   curl http://localhost:5000/health
   ```

3. Review logs for errors:
   ```powershell
   docker compose logs -f ledger-api
   ```

## Performance Optimization

### Docker Image Optimization

Current image uses multi-stage build:
- Build stage: SDK image (larger)
- Runtime stage: ASP.NET Runtime image (smaller)

Result: ~200MB final image

### Database Performance

```sql
-- Check database size
SELECT pg_size_pretty(pg_database_size('ledger_db'));

-- Check table indexes
SELECT schemaname, tablename, indexname 
FROM pg_indexes 
WHERE schemaname = 'public';

-- Vacuum database
VACUUM ANALYZE;
```

## Production Deployment

### Security Checklist

- [ ] Change default PostgreSQL password
- [ ] Use secrets management (Docker Secrets, Azure Key Vault)
- [ ] Enable HTTPS with valid certificates
- [ ] Restrict CORS origins
- [ ] Configure firewall rules
- [ ] Enable audit logging
- [ ] Regular backups
- [ ] Update base images regularly

### Environment-Specific Compose Files

Create `docker-compose.prod.yml`:

```yaml
services:
  ledger-api:
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
    deploy:
      replicas: 3
      resources:
        limits:
          cpus: '2'
          memory: 2G
```

Run with:
```powershell
docker compose -f docker-compose.yml -f docker-compose.prod.yml up -d
```

## Next Steps

- See [QUICK_START.md](QUICK_START.md) for API usage
- See [ARCHITECTURE.md](ARCHITECTURE.md) for system design
- Review Prometheus metrics: http://localhost:9090

## Support

- GitHub Issues: https://github.com/volcanion-company/volcanion-ledger-service/issues
- Email: support@volcanion.com
