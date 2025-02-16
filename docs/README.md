# ExpenseSplitter Project Documentation

## Project Overview

ExpenseSplitter is a microservices-based application for managing shared expenses and transactions between users. The application follows Clean Architecture principles and is built using .NET.

### Key Features

- User Authentication & Authorization
- Expense Management & Splitting
- Transaction Processing
- Audit Trail
- Soft Delete Support
- Value Objects for Domain Logic

### Architecture

- **Clean Architecture**
- **Microservices**
- **Domain-Driven Design (DDD)**
- **CQRS Pattern** (prepared for)

## Project Structure

ExpenseSplitter/
├── src/
│ ├── AuthService/
│ │ ├── API
│ │ ├── Application
│ │ ├── Domain
│ │ └── Infrastructure
│ ├── ExpenseService/
│ │ ├── API
│ │ ├── Application
│ │ ├── Domain
│ │ └── Infrastructure
│ ├── TransactionService/
│ │ ├── API
│ │ ├── Application
│ │ ├── Domain
│ │ └── Infrastructure
│ └── SharedLibrary/
Projects

dotnet tool update --global dotnet-ef

dotnet ef migrations add InitialCreate --startup-project ../AuthService.API

dotnet ef migrations add CheckUpdate --startup-project src/AuthService/AuthService.API --project src/AuthService/AuthService.Infrastructure/AuthService.Infrastructure.csproj

dotnet ef migrations add AddIdentitySupport --project src/AuthService/AuthService.Infrastructure/AuthService.Infrastructure.csproj --startup-project src/AuthService/AuthService.API/AuthService.API.csproj

dotnet ef database update --startup-project ../AuthService.API

dotnet ef database update --startup-project src/AuthService/AuthService.API

dotnet run --launch-profile https

dotnet watch --project src/AuthService/AuthService.API/AuthService.API.csproj --launch-profile https

# Documentation

This directory contains documentation for various aspects of the application.

## Contents

1. [Globalization Service Patterns](globalization-service-patterns.md)
   - Comprehensive guide to different approaches for accessing IGlobalizationService
   - Includes implementation details, pros and cons, and best practices
   - Covers 8 different patterns with code examples

## How to Use

1. Browse the documentation files directly on GitHub
2. Clone the repository to read offline
3. Use the table of contents in each document to navigate to specific sections

## Contributing

When adding new documentation:

1. Create a new markdown file
2. Add a link to it in this README
3. Follow the existing format and structure
4. Include code examples where appropriate
5. Add a table of contents for longer documents
