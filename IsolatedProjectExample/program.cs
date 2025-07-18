using Microsoft.Extensions.DependencyInjection;
using IsolateProject.Queries;
using IsolateProject.Handlers;
using System.Reflection;
using MediatR;

namespace IsolatedProjectExample
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("IsolatedProject Example - Basic MediatR Setup");
            Console.WriteLine("=============================================");

            var mediator = BuildMediator();
            await RunTests(mediator);
        }

        private static IMediator BuildMediator()
        {
            var services = new ServiceCollection();


            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssemblies(Assembly.Load("IsolateProject"));
            });


            var provider = services.BuildServiceProvider();

            return provider.GetRequiredService<IMediator>();
        }

        private static async Task RunTests(IMediator mediator)
        {
            Console.WriteLine("Testing GetUserQuery...");
            
            var query = new GetUserQuery { UserId = 123 };
            var response = await mediator.Send(query);
            
            Console.WriteLine($"User ID: {response.Id}");
            Console.WriteLine($"User Name: {response.Name}");
            Console.WriteLine($"User Email: {response.Email}");
            
            Console.WriteLine("\nTest completed successfully!");
        }
    }
}
