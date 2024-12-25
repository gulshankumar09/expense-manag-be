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
dotnet ef database update --startup-project src/AuthService/AuthService.API

# Run the application with HTTPS launch profile
dotnet run --launch-profile https

# Watch for changes and run the AuthService API project
dotnet watch --project src/AuthService/AuthService.API/AuthService.API.csproj --launch-profile https