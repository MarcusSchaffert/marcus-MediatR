# MediatR.Simple

A lightweight, simplified fork of MediatR that focuses exclusively on the core functionality you need: **IRequest**, **IRequestHandler**, and seamless .NET Core dependency injection integration.

## Why MediatR.Simple?

The original MediatR library includes many features like notifications, pipeline behaviors, streaming, and complex publishing strategies. If you only need the basic request/response pattern with automatic handler resolution, MediatR.Simple provides a much smaller, focused solution.

## Features

✅ **IRequest<TResponse>** - Requests with a response  
✅ **IRequest** - Requests without a response (commands)  
✅ **IRequestHandler<TRequest, TResponse>** - Handlers for requests with responses  
✅ **IRequestHandler<TRequest>** - Handlers for requests without responses  
✅ **Automatic dependency injection** - Automatic handler discovery and registration  
✅ **Multi-assembly scanning** - Scan handlers from multiple assemblies  
✅ **Thread-safe caching** - Efficient handler resolution with concurrent dictionary caching  
✅ **.NET 8 support** - Built for modern .NET  

❌ **No notifications** - Simplified to focus on request/response only  
❌ **No pipeline behaviors** - No middleware/behavior pipeline  
❌ **No streaming** - No IStreamRequest support  
❌ **No publishers** - No notification publishing strategies  

## Installation

```bash
# If published as NuGet package
dotnet add package MediatR.Simple

# Or include the source files directly in your project
```

## Quick Start

### 1. Define your requests and responses

```csharp
using MediatR.Simple;

// Query (request with response)
public class GetUserQuery : IRequest<User>
{
    public int UserId { get; set; }
}

public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}

// Command (request without response)
public class CreateUserCommand : IRequest
{
    public string Name { get; set; }
    public string Email { get; set; }
}
```

### 2. Create your handlers

```csharp
using MediatR.Simple;

// Handler for query
public class GetUserQueryHandler : IRequestHandler<GetUserQuery, User>
{
    public Task<User> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        // Your logic here
        var user = new User 
        { 
            Id = request.UserId, 
            Name = "John Doe", 
            Email = "john.doe@example.com" 
        };
        
        return Task.FromResult(user);
    }
}

// Handler for command
public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand>
{
    public Task Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // Your logic here
        Console.WriteLine($"Creating user: {request.Name}");
        return Task.CompletedTask;
    }
}
```

### 3. Register with dependency injection

```csharp
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

// Register MediatR.Simple and automatically discover handlers
services.AddMediatRSimple<Program>(); // Scans the assembly containing Program

// Or specify assemblies explicitly
services.AddMediatRSimple(typeof(GetUserQuery).Assembly);

// Or scan multiple assemblies
services.AddMediatRSimple(
    typeof(UserHandlers).Assembly,
    typeof(OrderHandlers).Assembly
);

var serviceProvider = services.BuildServiceProvider();
```

### 4. Use the mediator

```csharp
var mediator = serviceProvider.GetRequiredService<IMediator>();

// Send query
var query = new GetUserQuery { UserId = 123 };
var user = await mediator.Send(query);

// Send command  
var command = new CreateUserCommand { Name = "Jane", Email = "jane@example.com" };
await mediator.Send(command);
```

## ASP.NET Core Integration

```csharp
var builder = WebApplication.CreateBuilder(args);

// Register MediatR.Simple
builder.Services.AddMediatRSimple<Program>();

var app = builder.Build();

// Use in controllers
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;
    
    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpGet("{id}")]
    public async Task<User> GetUser(int id)
    {
        var query = new GetUserQuery { UserId = id };
        return await _mediator.Send(query);
    }
    
    [HttpPost]
    public async Task CreateUser(CreateUserCommand command)
    {
        await _mediator.Send(command);
    }
}
```

## Multi-Assembly Handler Registration

MediatR.Simple supports scanning handlers from multiple assemblies:

```csharp
// Scan handlers from multiple assemblies
services.AddMediatRSimple(
    typeof(Program).Assembly,           // Main application assembly
    typeof(OrderHandler).Assembly,      // Order handlers assembly
    typeof(PaymentHandler).Assembly     // Payment handlers assembly
);

// Alternative using type markers
services.AddMediatRSimple<Program>();                    // Main assembly
services.AddMediatRSimple(typeof(OrderHandler));         // Additional assembly
services.AddMediatRSimple(typeof(PaymentHandler));       // Another assembly
```

## Code Size Comparison

**Original MediatR**: ~150+ files, complex pipeline system, multiple patterns
**MediatR.Simple**: 6 core files, focused on request/response only

## Performance

- **Thread-safe caching**: Handler wrappers are cached using `ConcurrentDictionary`
- **Minimal allocations**: Simple design with fewer abstractions
- **Direct service resolution**: Efficient dependency injection integration

## When to Use MediatR.Simple

✅ **Use when you need:**
- Simple request/response pattern (CQRS)
- Automatic handler discovery and registration
- Multi-assembly handler scanning
- Clean separation between controllers and business logic
- Lightweight mediator pattern implementation

❌ **Don't use when you need:**
- Event publishing/notifications
- Pipeline behaviors or middleware
- Streaming responses
- Complex publishing strategies
- The full MediatR feature set

## Contributing

This is a simplified fork focused on essential functionality. Feature requests should align with the core philosophy of keeping it simple and lightweight.

## License

[Same as original MediatR - Apache 2.0]
