using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace MediatR.Simple.Tests;

public class ExceptionTests
{
    public class ThrowingRequest : IRequest<string>
    {
        public string ExceptionMessage { get; set; } = string.Empty;
    }

    public class ThrowingVoidRequest : IRequest
    {
        public string ExceptionMessage { get; set; } = string.Empty;
    }

    public class ThrowingRequestHandler : IRequestHandler<ThrowingRequest, string>
    {
        public Task<string> Handle(ThrowingRequest request, CancellationToken cancellationToken)
        {
            throw new InvalidOperationException(request.ExceptionMessage);
        }
    }

    public class ThrowingVoidRequestHandler : IRequestHandler<ThrowingVoidRequest>
    {
        public Task Handle(ThrowingVoidRequest request, CancellationToken cancellationToken)
        {
            throw new InvalidOperationException(request.ExceptionMessage);
        }
    }

    public class UnregisteredRequest : IRequest<string>
    {
    }

    [Fact]
    public async Task Should_propagate_exceptions_from_request_handler()
    {
        var services = new ServiceCollection();
        services.AddMediatRSimple<ExceptionTests>();
        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        var request = new ThrowingRequest { ExceptionMessage = "Test exception" };
        
        var exception = await Should.ThrowAsync<InvalidOperationException>(() => mediator.Send(request));
        exception.Message.ShouldBe("Test exception");
    }

    [Fact]
    public async Task Should_propagate_exceptions_from_void_request_handler()
    {
        var services = new ServiceCollection();
        services.AddMediatRSimple<ExceptionTests>();
        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        var request = new ThrowingVoidRequest { ExceptionMessage = "Void test exception" };
        
        var exception = await Should.ThrowAsync<InvalidOperationException>(() => mediator.Send(request));
        exception.Message.ShouldBe("Void test exception");
    }

    [Fact]
    public void Should_throw_when_handler_not_registered()
    {
        var services = new ServiceCollection();
        services.AddMediatRSimple<ExceptionTests>(); // This won't register UnregisteredRequest handler
        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        var request = new UnregisteredRequest();
        
        Should.Throw<InvalidOperationException>(async () => await mediator.Send(request));
    }

    [Fact]
    public async Task Should_handle_cancellation_token()
    {
        var services = new ServiceCollection();
        services.AddMediatRSimple<ExceptionTests>();
        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var request = new CancellationRequest();
        
        await Should.ThrowAsync<OperationCanceledException>(() => mediator.Send(request, cts.Token));
    }

    public class CancellationRequest : IRequest<string>
    {
    }

    public class CancellationRequestHandler : IRequestHandler<CancellationRequest, string>
    {
        public async Task<string> Handle(CancellationRequest request, CancellationToken cancellationToken)
        {
            await Task.Delay(1000, cancellationToken); // This should throw OperationCanceledException
            return "Should not reach here";
        }
    }
}
