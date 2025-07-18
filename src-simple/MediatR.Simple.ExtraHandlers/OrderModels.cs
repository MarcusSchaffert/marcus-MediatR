using System;
using MediatR.Simple;

namespace MediatR.Simple.ExtraHandlers;

// Order model
public class Order
{
    public int Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTime OrderDate { get; set; }
}

// Query to get order by ID
public class GetOrderQuery : IRequest<Order>
{
    public int OrderId { get; set; }
}

// Command to create a new order
public class CreateOrderCommand : IRequest<int>
{
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

// Command to delete an order (no response)
public class DeleteOrderCommand : IRequest
{
    public int OrderId { get; set; }
}
