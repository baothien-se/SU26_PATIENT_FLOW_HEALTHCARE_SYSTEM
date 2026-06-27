# Hospital Management System

> A comprehensive microservices-based hospital management system built with Clean Architecture, CQRS, DDD, and event-driven architecture.

## 📁 Project Structure

```
Clean_Architecture_Microservice/
├── docs/                              # 📚 Documentation
│   ├── SYSTEM_DESIGN.md              # Complete system architecture
│   ├── PHASE_1_COMPLETION.md         # Phase 1 status & deliverables
│   ├── DEVELOPER_QUICK_REFERENCE.md  # Quick start guide for new services
│   └── WORKFLOW_EXPLANATION.md       # Detailed workflow & component explanation ⭐
│
├── src/
│   ├── Shared/                        # 🔧 Shared libraries (all services use)
│   │   ├── Shared.Domain/            # Base entities, aggregates, value objects
│   │   ├── Shared.Application/       # CQRS interfaces, repositories, DTOs
│   │   ├── Shared.Infrastructure/    # Shared infrastructure implementations
│   │   └── Shared.Events/            # Integration events for RabbitMQ
│   │
│   ├── Gateways/
│   │   └── ApiGateway/               # YARP reverse proxy (entry point)
│   │
│   └── Services/
│       ├── IdentityService/          # Authentication & Authorization (JWT)
│       │   ├── IdentityService.Api
│       │   ├── IdentityService.Application
│       │   ├── IdentityService.Domain
│       │   └── IdentityService.Infrastructure
│       │
│       ├── UserService/              # 🆕 Staff management (RBAC) [Phase 2]
│       │
│       ├── PatientRegistrationService/ # 🆕 Patient registration & medical history [Phase 2]
│       │
│       ├── AppointmentService/       # 🆕 Appointment scheduling & queue [Phase 2]
│       │
│       ├── ConsultationService/      # 🆕 Doctor dashboard & consultation [Phase 3]
│       ├── LabService/               # 🆕 Lab test management [Phase 3]
│       ├── ImagingService/           # 🆕 Medical imaging [Phase 3]
│       ├── SurgeryService/           # 🆕 Surgery management [Phase 3]
│       ├── PharmacyService/          # 🆕 Medicine management [Phase 4]
│       ├── BillingService/           # 🆕 Payment & invoicing [Phase 4]
│       └── NotificationService/      # 🆕 Alerts & messaging [Phase 4]
│
├── Database/
│   └── 001_HospitalManagementSchema.sql  # SQL Server database schema
│
├── start-all.ps1                     # 🚀 Start all services (one command!)
├── CleanMicroservices.slnx           # Visual Studio solution file
└── README.md                         # This file

```

---

## 🚀 Quick Start

### Prerequisites

- **.NET 10.0** SDK or higher
- **SQL Server** 2019+ (LocalDB or full installation)
- **RabbitMQ** 3.12+ (optional, for async messaging - can use Docker)
- **Docker** (recommended, for dependencies)

### 1. Setup Database

```powershell
# Create database and run schema script
# Open SQL Server Management Studio and execute:
# Database/001_HospitalManagementSchema.sql
```

### 2. Start All Services

```powershell
cd Clean_Architecture_Microservice
.\start-all.ps1

# Or specific mode:
.\start-all.ps1 -Mode full        # Start everything
.\start-all.ps1 -Mode services    # Only microservices
.\start-all.ps1 -Mode gateway     # Only API gateway
```

### 3. Access Services

| Service             | HTTP                  | HTTPS                  | Docs                                  |
| ------------------- | --------------------- | ---------------------- | ------------------------------------- |
| **API Gateway**     | http://localhost:5000 | https://localhost:5001 | http://localhost:5000/swagger-ui.html |
| **IdentityService** | http://localhost:5010 | https://localhost:5011 | https://localhost:5011/scalar/v1      |
| **ProductService**  | http://localhost:5020 | https://localhost:5021 | https://localhost:5021/scalar/v1      |

---

## 📚 Documentation

Read these in order:

1. **[WORKFLOW_EXPLANATION.md](docs/WORKFLOW_EXPLANATION.md)** ⭐ **START HERE**
   - Explains each component (Entity, Repository, etc.)
   - Real-world workflow examples
   - Service communication patterns
   - Data flow diagrams

2. **[SYSTEM_DESIGN.md](docs/SYSTEM_DESIGN.md)**
   - Complete system architecture
   - Service specifications
   - Database schema overview
   - API routing design

3. **[PHASE_1_COMPLETION.md](docs/PHASE_1_COMPLETION.md)**
   - What was implemented in Phase 1
   - Technology stack
   - Project structure

4. **[DEVELOPER_QUICK_REFERENCE.md](docs/DEVELOPER_QUICK_REFERENCE.md)**
   - How to create a new service
   - Code patterns & examples
   - Common commands

---

## 🏗️ Architecture Patterns

### Clean Architecture

```
┌─────────────────────────────────────┐
│        Presentation Layer           │ (API Controllers)
├─────────────────────────────────────┤
│        Application Layer            │ (Commands, Queries, Handlers)
├─────────────────────────────────────┤
│        Domain Layer                 │ (Entities, Aggregates, Business Logic)
├─────────────────────────────────────┤
│        Infrastructure Layer         │ (Database, External Services)
└─────────────────────────────────────┘
```

### CQRS (Command Query Responsibility Segregation)

- **Commands**: Write operations (Create, Update, Delete)
- **Queries**: Read operations (Get, Search, Filter)
- Separated handlers for each, managed by MediatR

### DDD (Domain-Driven Design)

- **Entity**: Objects with unique identity
- **Aggregate Root**: Entry point for aggregate
- **Value Object**: Immutable objects with no identity
- **Domain Events**: Changes in domain state

### Event-Driven Architecture

- Services communicate via RabbitMQ
- Publishers emit events when state changes
- Subscribers handle events independently
- Loose coupling, high scalability

---

## 🔄 Service Communication

### Synchronous (HTTP/REST)

```
Client → API Gateway → Service A → Service B → Response
```

**Use when**: Need immediate response

### Asynchronous (RabbitMQ)

```
Service A → RabbitMQ → Service B
        → Service C
        → Service D
```

**Use when**: Can process later, need scalability

---

## 🗄️ Database

**Type**: SQL Server  
**Connection String**: See `appsettings.json` in each service

**Key Tables**:

- **Identity**: Users, Roles, Permissions
- **Patients**: Patient info, Medical history, Allergies
- **Appointments**: Scheduling, Queue management
- **Consultations**: Doctor visits
- **Lab**: Lab tests, Results
- **Imaging**: Medical imaging, Reports
- **Surgery**: Operations, Schedules
- **Pharmacy**: Prescriptions, Medicines
- **Billing**: Invoices, Payments

---

## 📊 Technology Stack

| Layer              | Technology            | Version        |
| ------------------ | --------------------- | -------------- |
| **Language**       | C#                    | Latest         |
| **Runtime**        | .NET                  | 10.0           |
| **Web Framework**  | ASP.NET Core          | 10.0           |
| **ORM**            | Entity Framework Core | 10.0           |
| **CQRS**           | MediatR               | 12.4.0         |
| **Validation**     | FluentValidation      | 11.10.0        |
| **API Gateway**    | YARP                  | 2.3.0          |
| **Authentication** | JWT Bearer            | 10.0           |
| **Message Bus**    | RabbitMQ              | 6.1.0          |
| **Real-time**      | SignalR               | 10.0 (Phase 3) |
| **Logging**        | Serilog               | 4.0.1          |
| **Mapping**        | AutoMapper            | 13.0.1         |
| **Database**       | SQL Server            | 2019+          |

---

## 🎯 Development Workflow

### Creating a New Service

1. Read [DEVELOPER_QUICK_REFERENCE.md](docs/DEVELOPER_QUICK_REFERENCE.md)
2. Create 4 projects (Domain, Application, Infrastructure, Api)
3. Add shared library references
4. Implement entities & aggregates
5. Implement commands & queries
6. Implement repositories & DbContext
7. Add to solution file
8. Update API Gateway routes

### Example: Creating UserService

```powershell
# See DEVELOPER_QUICK_REFERENCE.md for detailed steps
cd src/Services/UserService
dotnet new webapi -n UserService.Api
dotnet new classlib -n UserService.Application
# ... (more steps)
```

---

## 📋 Implementation Phases

### ✅ Phase 1: Foundation (COMPLETED)

- Shared libraries (Domain, Application, Infrastructure, Events)
- Database schema
- Api Gateway
- Identity Service

### 🔄 Phase 2: Core Services (CURRENT)

- UserService (Staff management & RBAC)
- PatientRegistrationService (Patient workflow)
- AppointmentService (Scheduling & queue)

### 📅 Phase 3: Clinical Services

- ConsultationService (Doctor dashboard)
- LabService (Lab tests)
- ImagingService (Medical imaging)
- SurgeryService (Operations)

### 🏥 Phase 4: Support Services

- PharmacyService (Medicines)
- BillingService (Payments)
- NotificationService (Alerts)

### 🔒 Phase 5: Advanced Features

- RabbitMQ integration across all services
- SignalR for real-time updates
- Kubernetes deployment
- Comprehensive testing

---

## 🔐 Security

### Authentication

- JWT Token-based (Identity Service)
- Issued on login, verified on each request
- Claims-based authorization

### Authorization

- Role-Based Access Control (RBAC)
- Permissions assigned per role
- Endpoints decorated with `[Authorize(Roles = "Doctor")]`

### Example

```csharp
[HttpPost("consultations")]
[Authorize(Roles = "Doctor")]
public async Task<IActionResult> CreateConsultation(...)
{
    // Only doctors can access
}
```

---

## 🧪 Testing

Each service includes unit & integration tests:

```powershell
# Run all tests
dotnet test

# Run specific test project
dotnet test src/Services/IdentityService/IdentityService.Application.Tests/
```

---

## 📈 Monitoring & Logging

Uses **Serilog** for structured logging:

```json
{
  "Serilog": {
    "MinimumLevel": "Information",
    "WriteTo": [
      { "Name": "Console" },
      {
        "Name": "File",
        "Args": { "path": "logs/app-.txt" }
      }
    ]
  }
}
```

---

## 🚀 Deployment

### Docker Compose (Local Development)

```bash
docker-compose up -d
```

### Kubernetes (Production)

```bash
kubectl apply -f k8s/
```

---

## 📞 Support & Contribution

For questions or improvements:

1. Check documentation in `docs/` folder
2. Read code comments in classes
3. Check Git commit history for rationale

---

## 📝 License

This project is for educational purposes.

---

## 🎓 Learning Resources

This codebase demonstrates:

- ✅ Clean Architecture principles
- ✅ Domain-Driven Design (DDD)
- ✅ CQRS Pattern
- ✅ Event-Driven Architecture
- ✅ Microservices Design
- ✅ Best practices in .NET development

**Perfect for learning**: Architecture, design patterns, scalable system design

---

**Last Updated**: 2026-06-28  
**Status**: Phase 1 Complete, Phase 2 In Progress 🚀
