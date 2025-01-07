# Authentication Service

The Authentication Service is a microservice responsible for user authentication, authorization, and role management in the Expense Splitter application.

## Features

### User Management
- User registration and login
- Email verification
- Password reset functionality
- Google OAuth integration
- Two-factor authentication support
- User profile management

### Role Management
- Role-based access control (RBAC)
- Hierarchical role structure:
  - SuperAdmin: Highest level of access, can manage all roles
  - Admin: Can manage users and assign non-SuperAdmin roles
  - User: Basic access level
- Configurable SuperAdmin user limit
- Role assignment validation and authorization

### Security Features
- JWT-based authentication
- Refresh token mechanism
- Password hashing using BCrypt
- Email verification
- Account lockout after failed attempts
- HTTPS/SSL support

## Technical Stack

- .NET 9.0
- ASP.NET Core Identity
- Entity Framework Core
- SQL Server
- Docker support

## Project Structure

```
AuthService/
├── AuthService.API/          # API endpoints and configuration
├── AuthService.Application/  # Business logic and services
├── AuthService.Domain/       # Domain entities and interfaces
└── AuthService.Infrastructure/ # Data access and external services
```

## Configuration

### Environment Variables
Create a `.env` file in the root directory with the following variables:
```
SQL_PASSWORD=your_secure_password
ASPNETCORE_ENVIRONMENT=Development
```

### Role Settings
The role management system can be configured through `appsettings.json`:
```json
{
  "RoleSettings": {
    "MaxSuperAdminUsers": 1
  }
}
```

## API Endpoints

### Authentication
- POST `/api/auth/register` - Register a new user
- POST `/api/auth/login` - User login
- POST `/api/auth/refresh-token` - Refresh JWT token
- POST `/api/auth/verify-email` - Verify email address
- POST `/api/auth/forgot-password` - Initiate password reset
- POST `/api/auth/reset-password` - Reset password
- POST `/api/auth/google-login` - Google OAuth login

### Role Management
- POST `/api/roles/create` - Create a new role (SuperAdmin only)
- POST `/api/roles/assign` - Assign role to user (SuperAdmin/Admin)
- GET `/api/roles/list` - List all roles
- GET `/api/roles/user/{userId}` - Get user's roles
- POST `/api/roles/superadmin-limit` - Update SuperAdmin user limit

## Setup and Installation

1. Prerequisites:
   - Docker and Docker Compose
   - .NET 9.0 SDK (for development)

2. Clone the repository:
   ```bash
   git clone <repository-url>
   cd expense-manag-be
   ```

3. Create and configure the `.env` file:
   ```bash
   cp .env.example .env
   # Edit .env with your settings
   ```

4. Start the services:
   ```bash
   docker compose up --build
   ```

5. The service will be available at:
   - HTTP: http://localhost:5000
   - HTTPS: https://localhost:5001
   - Swagger UI: https://localhost:5001/swagger

## Development

### Database Migrations
To create a new migration:
```bash
dotnet ef migrations add MigrationName --project src/AuthService/AuthService.Infrastructure --startup-project src/AuthService/AuthService.API
```

To apply migrations:
```bash
dotnet ef database update --project src/AuthService/AuthService.Infrastructure --startup-project src/AuthService/AuthService.API
```

### Running Tests
```bash
dotnet test src/AuthService/AuthService.Tests
```

## Security Considerations

- Always use HTTPS in production
- Keep the JWT secret key secure
- Regularly rotate refresh tokens
- Monitor failed login attempts
- Implement rate limiting for API endpoints
- Keep dependencies up to date

## Contributing

1. Create a feature branch
2. Make your changes
3. Submit a pull request

## License

[Your License] 