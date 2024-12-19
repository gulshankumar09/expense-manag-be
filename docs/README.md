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

dotnet ef database update --startup-project ../AuthService.API


dotnet run --launch-profile https
