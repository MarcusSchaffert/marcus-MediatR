using System.Threading;
using System.Threading.Tasks;
using IsolateProject.Queries;
using IsolateProject.Types;

namespace IsolateProject.Handlers
{
    public class GetUserQueryHandler : IRequestHandler<GetUserQuery, UserResponse>
    {
        public async Task<UserResponse> Handle(GetUserQuery request, CancellationToken cancellationToken = default)
        {
            // Simulate async operation
            await Task.Delay(100, cancellationToken);
            
            // Mock user data - in real implementation, this would query a database
            return new UserResponse
            {
                Id = request.UserId,
                Name = $"User {request.UserId}",
                Email = $"user{request.UserId}@example.com"
            };
        }
    }
}
