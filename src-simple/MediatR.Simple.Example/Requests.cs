using MediatR.Simple;

namespace MediatR.Simple.Example;

// Query example - request with response
public class GetUserQuery : IRequest<User>
{
    public int UserId { get; set; }
}

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

// Command example - request without response
public class CreateUserCommand : IRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
