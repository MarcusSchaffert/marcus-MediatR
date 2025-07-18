using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Simple;

namespace MediatR.Simple.ExtraHandlers;

// Handler for getting an order
public class GetOrderQueryHandler : IRequestHandler<GetOrderQuery, Order>
{
    public Task<Order> Handle(GetOrderQuery request, CancellationToken cancellationToken)
    {
        // Simulate getting order from database
        var order = new Order
        {
            Id = request.OrderId,
            ProductName = "Sample Product",
            Price = 99.99m,
            OrderDate = DateTime.UtcNow.AddDays(-1)
        };

        Console.WriteLine($"[ExtraHandlers] Retrieved order: {order.Id} - {order.ProductName}");
        return Task.FromResult(order);
    }
}

// Handler for creating an order (returns new order ID)
public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, int>
{
    private static int _nextId = 1000;

    public Task<int> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // Simulate creating order in database
        var newOrderId = ++_nextId;
        Console.WriteLine($"[ExtraHandlers] Created order {newOrderId}: {request.ProductName} - ${request.Price}");
        
        return Task.FromResult(newOrderId);
    }
}

// Handler for deleting an order (no response)
public class DeleteOrderCommandHandler : IRequestHandler<DeleteOrderCommand>
{
    public Task Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
    {
        // Simulate deleting order from database
        Console.WriteLine($"[ExtraHandlers] Deleted order: {request.OrderId}");
        
        return Task.CompletedTask;
    }
}
