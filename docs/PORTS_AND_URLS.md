# Ports and URLs Configuration

This document provides a comprehensive list of ports and URLs used across different environments in the AuthService microservice.

## Development Environment

### API Endpoints
- HTTP URL: `http://localhost:5000`
- HTTPS URL: `https://localhost:5001`
- Swagger UI: `https://localhost:5001/swagger`
- Health Check: `https://localhost:5001/health`

### Authentication
- JWT Issuer: `https://localhost:8000`
- JWT Audience: `https://localhost:8000`

### Database
- SQL Server: `localhost:1433`
- Connection String: `Server=(localdb)\\mssqllocaldb;Database=ExpenseSplitter.Auth;Trusted_Connection=True;MultipleActiveResultSets=true`

### Caching
- Redis: `localhost:6379`

## Docker Development Environment

### API Endpoints
- HTTP URL: `http://localhost:5002`
- HTTPS URL: `https://localhost:5003`
- Swagger UI: `https://localhost:5003/swagger`
- Health Check: `https://localhost:5003/health`

### Authentication
- JWT Issuer: `https://localhost:5001`
- JWT Audience: `https://localhost:5001`

### Database
- SQL Server: `Server=sqlserver;Database=ExpenseSplitter.Auth;User Id=sa;Password=${SQL_PASSWORD};TrustServerCertificate=True`
- Port: `1433`

### Caching
- Redis: `redis:6379`

### Container Names
- API Service: `authservice`
- Database: `authservice-db`
- Redis: `authservice-redis`

## Production Environment

### API Endpoints
- HTTP URL: Configured via environment variable
- HTTPS URL: Configured via environment variable
- Swagger UI: Disabled in production
- Health Check: `https://{domain}/health`

### Authentication
- JWT Issuer: Configured via environment variable
- JWT Audience: Configured via environment variable

### Database
- SQL Server: Configured via environment variable
- Connection String: Configured via environment variable

### Caching
- Redis: Configured via environment variable

## Environment Variables

### API Configuration
```env
ASPNETCORE_URLS=https://+:5001;http://+:5000
ASPNETCORE_ENVIRONMENT=Development|Production
ASPNETCORE_Kestrel__Certificates__Default__Path=/https/aspnetapp.pfx
ASPNETCORE_Kestrel__Certificates__Default__Password=${CERT_PASSWORD}
```

### Database Configuration
```env
SQL_PASSWORD=your_password_here
ConnectionStrings__DefaultConnection=your_connection_string_here
```

### JWT Configuration
```env
JWT_SECRET_KEY=your_secret_key_here
Jwt__Issuer=https://localhost:5001
Jwt__Audience=https://localhost:5001
Jwt__ExpiryInMinutes=60
```

### Email Configuration
```env
SMTP_HOST=smtp.gmail.com
SMTP_PORT=587
SMTP_USERNAME=your_email@gmail.com
SMTP_PASSWORD=your_app_specific_password
```

### Google OAuth Configuration
```env
GOOGLE_CLIENT_ID=your_client_id
GOOGLE_CLIENT_SECRET=your_client_secret
```

## Port Allocation

| Service    | Local Dev | Docker Dev | Production |
|------------|-----------|------------|------------|
| HTTP API   | 5000      | 5000       | Configurable |
| HTTPS API  | 5001      | 5001       | Configurable |
| SQL Server | 1433      | 1433       | Configurable |
| Redis      | 6379      | 6379       | Configurable |

## Network Configuration

### Docker Networks
- Network Name: `expense-network`
- Network Type: `bridge`

### Container DNS Names
- API Service: `authservice`
- Database: `sqlserver`
- Redis: `redis`

## SSL/TLS Configuration

### Development
- Certificate Path: `/https/aspnetapp.pfx`
- Certificate Password: Set via `CERT_PASSWORD` environment variable

### Production
- Certificate Path: Configured via environment variable
- Certificate Type: Valid SSL certificate (not self-signed)

## Health Checks

### Endpoints
- API Service: `https://localhost:5001/health`
- SQL Server: Internal health check
- Redis: Internal health check

### Intervals
- API Service: 30s interval, 10s timeout, 3 retries
- SQL Server: 10s interval, 5s timeout, 5 retries
- Redis: 10s interval, 5s timeout, 3 retries 