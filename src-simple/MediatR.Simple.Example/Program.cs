using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MediatR.Simple;
using MediatR.Simple.ExtraHandlers;
using System.Reflection;

namespace MediatR.Simple.Example;

class Program
{
    static async Task Main(string[] args)
    {
        // Setup dependency injection
        var services = new ServiceCollection();
        
        // Register MediatR.Simple with automatic handler discovery from multiple assemblies
        services.AddMediatRSimple(
            typeof(Program).Assembly,              // Current assembly (user handlers)
            //typeof(GetOrderQuery).Assembly,         // ExtraHandlers assembly (order handlers)
            Assembly.Load("MediatR.Simple.ExtraHandlers")
        );
        
        var serviceProvider = services.BuildServiceProvider();
        
        // Get the mediator
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        
        // Example 1: Query with response (from main assembly)
        Console.WriteLine("=== User Query Example (Main Assembly) ===");
        var getUserQuery = new GetUserQuery { UserId = 123 };
        var user = await mediator.Send(getUserQuery);
        Console.WriteLine($"Retrieved user: {user.Name} ({user.Email})");
        
        // Example 2: Command without response (from main assembly)
        Console.WriteLine("\n=== User Command Example (Main Assembly) ===");
        var createUserCommand = new CreateUserCommand 
        { 
            Name = "Jane Smith", 
            Email = "jane.smith@example.com" 
        };
        await mediator.Send(createUserCommand);
        
        // Example 3: Query with response (from extra handlers assembly)
        Console.WriteLine("\n=== Order Query Example (Extra Assembly) ===");
        var getOrderQuery = new GetOrderQuery { OrderId = 456 };
        var order = await mediator.Send(getOrderQuery);
        Console.WriteLine($"Retrieved order: {order.ProductName} - ${order.Price}");
        
        // Example 4: Command with response (from extra handlers assembly)
        Console.WriteLine("\n=== Create Order Command Example (Extra Assembly) ===");
        var createOrderCommand = new CreateOrderCommand 
        { 
            ProductName = "Amazing Widget", 
            Price = 149.99m 
        };
        var newOrderId = await mediator.Send(createOrderCommand);
        Console.WriteLine($"New order created with ID: {newOrderId}");
        
        // Example 5: Command without response (from extra handlers assembly)
        Console.WriteLine("\n=== Delete Order Command Example (Extra Assembly) ===");
        var deleteOrderCommand = new DeleteOrderCommand { OrderId = newOrderId };
        await mediator.Send(deleteOrderCommand);
        
        Console.WriteLine("\n=== Done ===");
    }
}
