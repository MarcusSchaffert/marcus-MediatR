using IsolateProject.Types;

namespace IsolateProject.Queries
{
    public class GetUserQuery : IRequest<UserResponse>
    {
        public int UserId { get; set; }
    }

    public class UserResponse
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
    }
}
