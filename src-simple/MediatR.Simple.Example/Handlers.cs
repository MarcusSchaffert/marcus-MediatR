using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Simple;

namespace MediatR.Simple.Example;

// Handler for the query
public class GetUserQueryHandler : IRequestHandler<GetUserQuery, User>
{
    public Task<User> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        // Simulate getting user from database
        var user = new User
        {
            Id = request.UserId,
            Name = "John Doe",
            Email = "john.doe@example.com"
        };

        return Task.FromResult(user);
    }
}

// Handler for the command
public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand>
{
    public Task Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // Simulate creating user in database
        Console.WriteLine($"Creating user: {request.Name} ({request.Email})");
        
        return Task.CompletedTask;
    }
}
