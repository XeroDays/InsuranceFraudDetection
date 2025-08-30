# Insurance Fraud Detection System

A comprehensive .NET 9.0 application for detecting and managing insurance fraud claims using modern software architecture patterns and real-time communication.

## 🚀 Features

### 🔍 Fraud Detection & Claims Management
- **Automated Fraud Detection**: AI-powered analysis of insurance claims
- **Real-time Processing**: Instant claim validation and fraud assessment
- **Comprehensive Claim Management**: Submit, review, and track insurance claims
- **User Management**: Secure user authentication and authorization

### 🏗️ Clean Architecture Implementation
- **Domain-Driven Design**: Well-structured domain entities and value objects
- **Separation of Concerns**: Clear separation between Application, Infrastructure, and Presentation layers
- **SOLID Principles**: Following clean architecture principles for maintainable code
- **Dependency Injection**: Proper IoC container configuration

### 📡 Real-time Communication
- **SignalR Integration**: Real-time fraud detection notifications
- **WebSocket Support**: Live updates for claim status changes
- **Hub-based Communication**: Centralized real-time messaging system

### 🔌 RESTful APIs
- **Claims API**: Complete CRUD operations for insurance claims
- **Health Check API**: System health monitoring endpoints
- **RESTful Design**: Standard HTTP methods and status codes
- **API Documentation**: Comprehensive API documentation

### 🧪 Comprehensive Testing
- **Unit Testing**: Extensive unit tests for business logic
- **Test Coverage**: High test coverage for critical components
- **Mocking**: Proper isolation of dependencies in tests
- **Test-Driven Development**: TDD approach for robust code

### 🔄 CI/CD Pipeline
- **Automated Builds**: Continuous integration with automated testing
- **Deployment Automation**: Streamlined deployment process
- **Quality Gates**: Automated quality checks and code analysis
- **Environment Management**: Multiple environment support (Development, Staging, Production)

## 🛠️ Technology Stack

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

## 🚀 Getting Started

### Prerequisites
- .NET 9.0 SDK
- Visual Studio 2022 or VS Code
- SQLite (included with EF Core)

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/yourusername/InsuranceFraudDetection.git
   cd InsuranceFraudDetection
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Run database migrations**
   ```bash
   dotnet ef database update
   ```

4. **Run the application**
   ```bash
   dotnet run
   ```

5. **Run tests**
   ```bash
   dotnet test
   ```

## 📊 API Endpoints

### Claims API
- `GET /api/claims` - Get all claims
- `POST /api/claims` - Submit a new claim
- `GET /api/claims/{id}` - Get claim by ID
- `PUT /api/claims/{id}` - Update claim
- `DELETE /api/claims/{id}` - Delete claim

### Health Check API
- `GET /api/health` - System health status

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

Run tests with coverage:
```bash
dotnet test --collect:"XPlat Code Coverage"
```

## 📈 CI/CD Pipeline

The project includes automated CI/CD pipeline with:
- **Build Automation**: Automated builds on every commit
- **Test Execution**: Automated test runs
- **Code Quality**: Static code analysis
- **Deployment**: Automated deployment to staging/production

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE.txt) file for details.

## 🆘 Support

For support and questions:
- Create an issue in the GitHub repository
- Contact the development team
- Check the documentation

## 🔮 Future Enhanced to be made

- [ ] Machine Learning integration for fraud detection 
- [ ] Advanced analytics dashboard
- [ ] Blockchain integration for claim verification

---

**Built with ❤️ using Clean Architecture and modern .NET practices**