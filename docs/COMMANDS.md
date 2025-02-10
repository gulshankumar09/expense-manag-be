# Update the dotnet-ef tool globally

dotnet tool update --global dotnet-ef

# Add initial migration for AuthService

dotnet ef migrations add InitialCreate --startup-project ../AuthService.API

# Add migration for AuthService with specific project

dotnet ef migrations add InitialCreate --startup-project src/AuthService/AuthService.API --project src/AuthService/AuthService.Infrastructure/AuthService.Infrastructure.csproj

# Add migration for AddIdentitySupport in AuthService

dotnet ef migrations add AddIdentitySupport --project src/AuthService/AuthService.Infrastructure/AuthService.Infrastructure.csproj --startup-project src/AuthService/AuthService.API/AuthService.API.csproj

# Update the database for AuthService

dotnet ef database update --startup-project ../AuthService.API

# Update the database for AuthService with specific project

```C#
dotnet ef database update --startup-project src/AuthService/AuthService.API

dotnet ef database update --project src/AuthService/AuthService.Infrastructure/AuthService.Infrastructure.csproj --startup-project src/AuthService/AuthService.API/AuthService.API.csproj
```

# Run the application with HTTPS launch profile

```C#
dotnet run --launch-profile https
```

# Watch for changes and run the AuthService API project

```C#
dotnet watch --project src/AuthService/AuthService.API/AuthService.API.csproj --launch-profile https
```

# Development mode

docker-compose up -d

# Production mode

docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d

docker-compose exec authservice dotnet ef database update

dotnet dev-certs https --clean
dotnet dev-certs https --trust
dotnet dev-certs https -ep ./certs/aspnetapp.pfx -p password

# first remove the existing migration and then apply it again to ensure it's properly updated:

dotnet ef database update 20250210183549_AddSuperAdminRole --project src/AuthService/AuthService.Infrastructure --startup-project src/AuthService/AuthService.API

dotnet ef database update --project src/AuthService/AuthService.Infrastructure --startup-project src/AuthService/AuthService.API
