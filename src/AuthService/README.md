# AuthService Microservice

## Overview
The AuthService is a microservice responsible for handling user authentication, authorization, and user management in the Expense Management system. It provides secure user authentication using JWT tokens and supports both email/password and Google OAuth 2.0 authentication methods.

## Features
- User Authentication (Email/Password)
- OAuth 2.0 Authentication (Google)
- JWT Token-based Authentication
- Role-based Authorization
- Email Verification using OTP
- Password Reset Functionality
- User Management
- Role Management

## Tech Stack
- .NET 9.0
- ASP.NET Core Web API
- Entity Framework Core
- Microsoft Identity
- SQL Server
- JWT Authentication
- SMTP Email Service

## Project Structure
```
AuthService/
├── AuthService.API/             # API Layer
├── AuthService.Application/     # Application Layer
├── AuthService.Domain/          # Domain Layer
└── AuthService.Infrastructure/  # Infrastructure Layer
```

### Clean Architecture
The project follows Clean Architecture principles with the following layers:
- **API Layer**: Controllers, Middleware, and API Configuration
- **Application Layer**: DTOs, Interfaces, Services, and Business Logic
- **Domain Layer**: Entities and Value Objects
- **Infrastructure Layer**: Database, External Services, and Repositories

## API Endpoints

### Account Management
- `POST /api/account/register` - Register a new user
- `POST /api/account/login` - User login
- `POST /api/account/verify-otp` - Verify email using OTP
- `POST /api/account/forgot-password` - Initiate password reset
- `POST /api/account/reset-password` - Reset password

### Role Management (Admin Only)
- `POST /api/roles/create` - Create a new role
- `POST /api/roles/assign` - Assign role to user
- `GET /api/roles/list` - List all roles
- `GET /api/roles/user/{userId}` - Get user's roles

## Authentication Flow

### Email/Password Registration
1. User registers with email/password
2. System sends OTP to user's email
3. User verifies email with OTP
4. User can now login

### Password Reset
1. User requests password reset
2. System sends reset link to email
3. User clicks link and sets new password

### JWT Token
- Access Token validity: 60 minutes
- Refresh Token validity: 7 days
- Token includes user claims (ID, Email, Roles)

## Configuration

### appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ExpenseSplitter.Auth;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "Jwt": {
    "Key": "your_secret_key_here",
    "Issuer": "http://localhost:5000",
    "Audience": "http://localhost:5000",
    "ExpiryInMinutes": 60
  },
  "Email": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "Username": "your_email@gmail.com",
    "Password": "your_app_specific_password",
    "EnableSsl": true,
    "FromEmail": "your_email@gmail.com",
    "FromName": "Expense Manager"
  },
  "Google": {
    "ClientId": "your_google_client_id",
    "ClientSecret": "your_google_client_secret"
  }
}
```

## Security Features
- Password Requirements:
  - Minimum length: 8 characters
  - Requires uppercase letter
  - Requires lowercase letter
  - Requires digit
  - Requires special character

- Account Security:
  - Email verification required
  - Account lockout after 5 failed attempts
  - Lockout duration: 5 minutes
  - Rate limiting: 10 requests per minute

## Database
- Uses SQL Server with Entity Framework Core
- Identity tables for user management
- Custom tables for additional features
- Soft delete support
- Audit trails (Created/Updated timestamps)

## Getting Started

### Prerequisites
- .NET 9.0 SDK
- SQL Server
- SMTP Server access (for email functionality)

### Setup
1. Clone the repository
2. Update connection string in appsettings.json
3. Run migrations:
   ```bash
   dotnet ef database update --project src/AuthService/AuthService.Infrastructure --startup-project src/AuthService/AuthService.API
   ```
4. Configure email settings in appsettings.json
5. Run the application:
   ```bash
   dotnet run --project src/AuthService/AuthService.API
   ```

### Default Roles
The system automatically creates two default roles on startup:
- Admin
- User

## Error Handling
- Uses Result pattern for consistent error responses
- Structured error messages
- Proper HTTP status codes
- Detailed logging with correlation IDs

## Rate Limiting
- Global rate limit: 10 requests per minute
- Custom rate limits for specific endpoints
- IP-based rate limiting

## Dependencies
```xml
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="9.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="9.0.0" />
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="9.0.0" />
<PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
```

## Contributing
1. Create a feature branch
2. Make your changes
3. Submit a pull request

## License
This project is licensed under the MIT License. 