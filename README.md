# Insurance Fraud Detection System

A comprehensive .NET 9.0 application for detecting and managing insurance fraud claims using modern software architecture patterns and real-time communication.

# 🚀 Features

## 🏗️ Clean Architecture Implementation
- **Domain-Driven Design**: Well-structured domain entities and value objects
- **SOLID Principles**: Following clean architecture principles for maintainable code

## 🔍 Fraud Detection & Claims Management
- **Real-time Processing**: Instant claim validation and fraud assessment


## 📡 SignalR Real-time Communication
- **SignalR Integration**: Real-time fraud detection notifications
- **WebSocket Support**: Live updates for claim status changes 

### 🔌 REST APIs
- **Claims API**: Complete CRUD operations for insurance claims
- **Health Check API**: System health monitoring endpoints

## 🧪 Comprehensive Testing
- **Unit Testing**: Extensive unit tests for business logic
- **Test-Driven Development**: TDD approach for robust code

## 🔄 CI/CD Pipeline
- **Automated Builds**: Continuous integration with automated testing
- **Deployment Automation**: Streamlined deployment process

## 🔄 Custom Logger
- **UnCaught Exception Handling**: Custom logger is capturing all uncuaght exceptions
- **Middleware Logging**: All requests are logged with middleware



# 🛠️ Technology Stack

- **Framework**: .NET 9.0
- **Database**: SQLite with Entity Framework Core
- **Real-time**: SignalR
- **Testing**: xUnit
- **Architecture**: Clean Architecture with CQRS pattern
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection

## 📁 Project Structure

```
InsuranceFraudDetection/
├── Application/           # Application layer (use cases, services)
│   ├── Claims/           # Claims-related application logic
│   └── Interfaces/       # Application interfaces
├── Core/                 # Domain layer (entities, value objects)
│   ├── Entities/         # Domain entities
│   ├── ValueObjects/     # Value objects
│   └── SharedKernel/     # Shared domain concepts
├── Infrastructure/       # Infrastructure layer
│   ├── Data/            # Database context and configurations
│   ├── Persistence/     # Repository implementations
│   ├── SignalR/         # Real-time communication
│   └── DependencyInjection/ # DI configuration
├── Presentation/         # Presentation layer (MVC)
│   ├── Controllers/     # MVC controllers
│   ├── Views/           # Razor views
│   └── Models/          # View models
├── WebAPI/              # API layer
│   ├── Controllers/     # API controllers
│   └── Models/          # API models
└── InsuranceFraudDetection.Tests/ # Test project
    └── Application/     # Application layer tests
```
  
 
## 🔧 Configuration

The application supports multiple environments with different configuration files:
- `appsettings.json` - Base configuration
- `appsettings.Development.json` - Development environment
- `appsettings.Staging.json` - Staging environment
- `appsettings.Production.json` - Production environment

## 🧪 Testing

Run the test suite:
```bash
dotnet test
```
 
## 📈 CI/CD Pipeline

The project includes automated CI/CD pipeline with:
- **Build Automation**: Automated builds on every commit
- **Test Execution**: Automated test runs
- **Code Quality**: Static code analysis
- **Deployment**: Automated deployment to staging/production
  

## 🔮 Future Enhanced to be made

- [ ] Machine Learning integration for fraud detection 
- [ ] Advanced analytics dashboard
- [ ] Blockchain integration for claim verification

--- 
