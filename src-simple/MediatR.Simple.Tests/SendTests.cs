using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace MediatR.Simple.Tests;

public class SendTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IMediator _mediator;
    private readonly TestDependency _dependency;

    public SendTests()
    {
        _dependency = new TestDependency();
        var services = new ServiceCollection();
        services.AddMediatRSimple<SendTests>();
        services.AddSingleton(_dependency);
        _serviceProvider = services.BuildServiceProvider();
        _mediator = _serviceProvider.GetRequiredService<IMediator>();
    }

    public class Ping : IRequest<Pong>
    {
        public string? Message { get; set; }
    }

    public class Pong
    {
        public string? Message { get; set; }
    }

    public class VoidPing : IRequest
    {
        public string? Message { get; set; }
    }

    public class TestDependency
    {
        public bool Called { get; set; }
        public string? LastMessage { get; set; }
    }

    public class PingHandler : IRequestHandler<Ping, Pong>
    {
        private readonly TestDependency _dependency;

        public PingHandler(TestDependency dependency)
        {
            _dependency = dependency;
        }

        public Task<Pong> Handle(Ping request, CancellationToken cancellationToken)
        {
            _dependency.Called = true;
            _dependency.LastMessage = request.Message;
            return Task.FromResult(new Pong { Message = request.Message + " Pong" });
        }
    }

    public class VoidPingHandler : IRequestHandler<VoidPing>
    {
        private readonly TestDependency _dependency;

        public VoidPingHandler(TestDependency dependency)
        {
            _dependency = dependency;
        }

        public Task Handle(VoidPing request, CancellationToken cancellationToken)
        {
            _dependency.Called = true;
            _dependency.LastMessage = request.Message;
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task Should_resolve_request_response_handler()
    {
        var request = new Ping { Message = "Hello" };
        var response = await _mediator.Send(request);

        response.Message.ShouldBe("Hello Pong");
        _dependency.Called.ShouldBeTrue();
        _dependency.LastMessage.ShouldBe("Hello");
    }

    [Fact]
    public async Task Should_resolve_void_request_handler()
    {
        var request = new VoidPing { Message = "Hello Void" };
        await _mediator.Send(request);

        _dependency.Called.ShouldBeTrue();
        _dependency.LastMessage.ShouldBe("Hello Void");
    }

    [Fact]
    public async Task Should_throw_when_request_is_null()
    {
        Ping request = null!;
        await Should.ThrowAsync<ArgumentNullException>(() => _mediator.Send(request));
    }

    [Fact]
    public async Task Should_throw_when_void_request_is_null()
    {
        VoidPing request = null!;
        await Should.ThrowAsync<ArgumentNullException>(() => _mediator.Send(request));
    }

    [Fact]
    public async Task Should_handle_multiple_requests_with_same_handler()
    {
        var request1 = new Ping { Message = "First" };
        var request2 = new Ping { Message = "Second" };

        var response1 = await _mediator.Send(request1);
        var response2 = await _mediator.Send(request2);

        response1.Message.ShouldBe("First Pong");
        response2.Message.ShouldBe("Second Pong");
    }

    [Fact]
    public void Should_throw_when_handler_not_registered()
    {
        var services = new ServiceCollection();
        services.AddMediatRSimple<SendTests>(); // Register without handlers
        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        var request = new UnhandledRequest();
        Should.Throw<InvalidOperationException>(async () => await mediator.Send(request));
    }

    public class UnhandledRequest : IRequest<string>
    {
    }
}
