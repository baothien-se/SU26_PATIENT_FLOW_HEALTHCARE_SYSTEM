# Hospital Management System - Developer Quick Reference

## 🚀 Bắt Đầu Một Service Mới

### 1. Tạo Project Structure

```powershell
# Từ thư mục gốc
cd src/Services/YourServiceName

# Tạo 4 projects theo Clean Architecture
dotnet new webapi -n "YourServiceName.Api"
dotnet new classlib -n "YourServiceName.Application"
dotnet new classlib -n "YourServiceName.Domain"
dotnet new classlib -n "YourServiceName.Infrastructure"
```

### 2. Thêm Project References

```powershell
# Application references Domain
cd YourServiceName.Application
dotnet add reference ..\YourServiceName.Domain

# Infrastructure references Application
cd ..\YourServiceName.Infrastructure
dotnet add reference ..\YourServiceName.Application

# Api references Infrastructure
cd ..\YourServiceName.Api
dotnet add reference ..\YourServiceName.Infrastructure

# Link tất cả to Shared libraries
dotnet add reference ..\..\..\..\Shared\Shared.Domain\Shared.Domain.csproj
dotnet add reference ..\..\..\..\Shared\Shared.Application\Shared.Application.csproj
dotnet add reference ..\..\..\..\Shared\Shared.Infrastructure\Shared.Infrastructure.csproj
dotnet add reference ..\..\..\..\Shared\Shared.Events\Shared.Events.csproj
```

### 3. Thêm NuGet Packages

```powershell
# Vào mỗi project và add packages:

# YourServiceName.Domain
dotnet add package Microsoft.EntityFrameworkCore --version 10.0.9

# YourServiceName.Application  
dotnet add package MediatR --version 12.4.0
dotnet add package FluentValidation --version 11.10.0
dotnet add package AutoMapper --version 13.0.1
dotnet add package AutoMapper.Extensions.Microsoft.DependencyInjection --version 12.0.1

# YourServiceName.Infrastructure
dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 10.0.9
dotnet add package RabbitMQ.Client --version 6.1.0
dotnet add package Serilog --version 4.0.1
dotnet add package Serilog.Sinks.Console --version 5.1.0

# YourServiceName.Api
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 10.0.9
dotnet add package Scalar.AspNetCore --version 2.16.6
dotnet add package Serilog.AspNetCore --version 8.1.1
```

---

## 📊 Domain Layer Pattern

### Tạo Entity

```csharp
// YourServiceName.Domain/Entities/YourEntity.cs
using Shared.Domain;

namespace YourServiceName.Domain.Entities;

public class YourEntity : Entity
{
    // Properties
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    
    // Constructor for EF Core
    public YourEntity() { }
    
    // Static factory method
    public static YourEntity Create(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name));
            
        return new YourEntity 
        { 
            Name = name, 
            Description = description 
        };
    }
    
    // Business methods
    public void UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Name cannot be empty");
            
        Name = newName;
        UpdateTimestamp();
    }
}
```

### Tạo Aggregate Root

```csharp
// YourServiceName.Domain/Aggregates/YourAggregate.cs
using Shared.Domain;

namespace YourServiceName.Domain.Aggregates;

public class YourAggregate : AggregateRoot
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    
    private YourAggregate() { }
    
    public static YourAggregate Create(string code, string name)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException("Code is required");
            
        var aggregate = new YourAggregate
        {
            Code = code,
            Name = name
        };
        
        // Raise domain event
        aggregate.RaiseDomainEvent(new YourAggregateCreatedEvent 
        { 
            AggregateId = aggregate.Id,
            Code = code,
            Name = name 
        });
        
        return aggregate;
    }
}

// Domain Event
public class YourAggregateCreatedEvent : DomainEvent
{
    public int AggregateId { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
}
```

### Tạo Value Object

```csharp
// YourServiceName.Domain/ValueObjects/PhoneNumber.cs
using Shared.Domain;

namespace YourServiceName.Domain.ValueObjects;

public class PhoneNumber : ValueObject
{
    public string Number { get; private set; }
    
    public PhoneNumber(string number)
    {
        if (string.IsNullOrWhiteSpace(number))
            throw new ArgumentNullException(nameof(number));
        Number = number;
    }
    
    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Number;
    }
    
    public override string ToString() => Number;
}
```

---

## 📝 Application Layer Pattern

### Tạo Command

```csharp
// YourServiceName.Application/Features/YourFeature/Commands/CreateYourCommand.cs
using MediatR;
using Shared.Domain;

namespace YourServiceName.Application.Features.YourFeature.Commands;

public record CreateYourCommand(string Name, string Description) : IRequest<Result<int>>;

public class CreateYourCommandValidator : AbstractValidator<CreateYourCommand>
{
    public CreateYourCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name cannot exceed 200 characters");
            
        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");
    }
}

public class CreateYourCommandHandler : IRequestHandler<CreateYourCommand, Result<int>>
{
    private readonly IRepository<YourEntity> _repository;
    private readonly IUnitOfWork _unitOfWork;
    
    public CreateYourCommandHandler(
        IRepository<YourEntity> repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }
    
    public async Task<Result<int>> Handle(CreateYourCommand request, CancellationToken cancellationToken)
    {
        var entity = YourEntity.Create(request.Name, request.Description);
        
        var created = await _repository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return Result<int>.Success(created.Id, "Entity created successfully");
    }
}
```

### Tạo Query

```csharp
// YourServiceName.Application/Features/YourFeature/Queries/GetYourQuery.cs
using MediatR;
using Shared.Domain;

namespace YourServiceName.Application.Features.YourFeature.Queries;

public record GetYourQuery(int Id) : IRequest<Result<YourDto>>;

public class GetYourQueryHandler : IRequestHandler<GetYourQuery, Result<YourDto>>
{
    private readonly IRepository<YourEntity> _repository;
    private readonly IMapper _mapper;
    
    public GetYourQueryHandler(IRepository<YourEntity> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }
    
    public async Task<Result<YourDto>> Handle(GetYourQuery request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);
        
        if (entity == null)
            return Result<YourDto>.Failure($"Entity with ID {request.Id} not found");
            
        var dto = _mapper.Map<YourDto>(entity);
        return Result<YourDto>.Success(dto);
    }
}

public record YourDto(int Id, string Name, string Description);
```

---

## 🔌 Infrastructure Layer Pattern

### DbContext Setup

```csharp
// YourServiceName.Infrastructure/Persistence/YourDbContext.cs
using Microsoft.EntityFrameworkCore;
using YourServiceName.Domain.Entities;

namespace YourServiceName.Infrastructure.Persistence;

public class YourDbContext : DbContext
{
    public YourDbContext(DbContextOptions<YourDbContext> options) 
        : base(options) { }
    
    public DbSet<YourEntity> YourEntities { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configure YourEntity
        modelBuilder.Entity<YourEntity>(builder =>
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
            builder.Property(x => x.Description).HasMaxLength(500);
            builder.HasQueryFilter(x => !x.IsDeleted);
            builder.HasIndex(x => x.CreatedAt);
        });
    }
}
```

### Repository Implementation

```csharp
// YourServiceName.Infrastructure/Persistence/Repository.cs
using Shared.Application.Interfaces;
using Shared.Domain;
using Microsoft.EntityFrameworkCore;

namespace YourServiceName.Infrastructure.Persistence;

public class Repository<TEntity> : IRepository<TEntity> 
    where TEntity : Entity
{
    protected readonly YourDbContext _dbContext;
    
    public Repository(YourDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public virtual async Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<TEntity>()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }
    
    public virtual async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<TEntity>().ToListAsync(cancellationToken);
    }
    
    public virtual async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<TEntity>().AddAsync(entity, cancellationToken);
        return entity;
    }
    
    public virtual async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _dbContext.Set<TEntity>().Update(entity);
        await Task.CompletedTask;
    }
    
    public virtual async Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        entity.Delete(); // Soft delete
        _dbContext.Set<TEntity>().Update(entity);
        await Task.CompletedTask;
    }
    
    // Implement other interface methods...
}
```

### Dependency Injection

```csharp
// YourServiceName.Infrastructure/DependencyInjection.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Application.Interfaces;

namespace YourServiceName.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register DbContext
        services.AddDbContext<YourDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly("YourServiceName.Infrastructure")));
        
        // Register Repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        return services;
    }
}
```

---

## 🌐 API Layer Pattern

### Program.cs Setup

```csharp
// YourServiceName.Api/Program.cs
using YourServiceName.Application;
using YourServiceName.Infrastructure;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services
    .AddApplicationServices()
    .AddInfrastructureServices(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddScalar("/api/docs");
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapScalar("/api/docs");
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();
app.Run();
```

### Controller Example

```csharp
// YourServiceName.Api/Controllers/YourController.cs
using MediatR;
using Microsoft.AspNetCore.Mvc;
using YourServiceName.Application.Features.YourFeature.Commands;
using YourServiceName.Application.Features.YourFeature.Queries;

namespace YourServiceName.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class YourController : ControllerBase
{
    private readonly IMediator _mediator;
    
    public YourController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateYourCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess 
            ? CreatedAtAction(nameof(GetById), new { id = result.Data }, result.Data)
            : BadRequest(result);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _mediator.Send(new GetYourQuery(id));
        return result.IsSuccess 
            ? Ok(result.Data)
            : NotFound(result);
    }
}
```

---

## 🔄 Publishing Integration Events

```csharp
// Trong Command Handler hoặc Domain Service
public class YourCommandHandler : IRequestHandler<YourCommand, Result<int>>
{
    private readonly IIntegrationEventPublisher _eventPublisher;
    private readonly IRepository<YourAggregate> _repository;
    
    public async Task<Result<int>> Handle(YourCommand request, CancellationToken cancellationToken)
    {
        // Create aggregate
        var aggregate = YourAggregate.Create(request.Code, request.Name);
        
        // Save
        var saved = await _repository.AddAsync(aggregate, cancellationToken);
        
        // Get uncommitted events
        var events = aggregate.GetUncommittedEvents();
        
        // Publish to RabbitMQ
        foreach (var @event in events)
        {
            // Convert domain event to integration event
            var integrationEvent = new YourCreatedEvent 
            { 
                AggregateId = aggregate.Id,
                Code = aggregate.Code,
                Name = aggregate.Name
            };
            
            await _eventPublisher.PublishAsync(
                integrationEvent,
                "your.service.created",
                cancellationToken);
        }
        
        return Result<int>.Success(saved.Id);
    }
}
```

---

## 📚 Common Integration Events

```csharp
// Sử dụng từ Shared.Events
using Shared.Events;

// Subscribe event
public class OnPatientRegistered : IIntegrationEventHandler<PatientRegisteredEvent>
{
    public async Task HandleAsync(PatientRegisteredEvent integrationEvent, CancellationToken cancellationToken)
    {
        // Handle patient registration event
        // E.g., create appointment slot, send welcome email, etc.
    }
}
```

---

## 🧪 Testing Patterns

```csharp
// Example Unit Test
[TestClass]
public class YourEntityTests
{
    [TestMethod]
    public void Create_WithValidData_ShouldCreateEntity()
    {
        // Arrange & Act
        var entity = YourEntity.Create("Test", "Description");
        
        // Assert
        Assert.AreEqual("Test", entity.Name);
        Assert.IsTrue(entity.CreatedAt != default);
    }
    
    [TestMethod]
    public void UpdateName_WithEmptyName_ShouldThrow()
    {
        // Arrange
        var entity = YourEntity.Create("Test", "Description");
        
        // Act & Assert
        Assert.ThrowsException<ArgumentException>(() => entity.UpdateName(""));
    }
}
```

---

## 🔌 Connection Strings

```json
// appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=YourServiceDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  },
  "JwtSettings": {
    "Secret": "YourSuperSecretKeyMinimum32CharactersLong!",
    "Issuer": "YourServiceName",
    "Audience": "HospitalClients",
    "ExpiryMinutes": "120"
  },
  "RabbitMQ": {
    "HostName": "localhost",
    "Port": 5672,
    "UserName": "guest",
    "Password": "guest",
    "VirtualHost": "/",
    "Exchange": "hospital.events"
  }
}
```

---

## 📖 Useful Commands

```powershell
# Add migration
dotnet ef migrations add InitialCreate -p YourServiceName.Infrastructure -s YourServiceName.Api

# Update database
dotnet ef database update -p YourServiceName.Infrastructure -s YourServiceName.Api

# Build
dotnet build

# Run
dotnet run --project YourServiceName.Api

# Run tests
dotnet test
```

---

**Next: Tạo Phase 2 Services! 🚀**
