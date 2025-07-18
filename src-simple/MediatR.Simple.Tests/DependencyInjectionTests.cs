using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace MediatR.Simple.Tests;

public class DependencyInjectionTests
{
    public class TestRequest : IRequest<string>
    {
        public string Value { get; set; } = string.Empty;
    }

    public class TestRequestHandler : IRequestHandler<TestRequest, string>
    {
        public Task<string> Handle(TestRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult($"Handled: {request.Value}");
        }
    }

    public class VoidTestRequest : IRequest
    {
        public string Value { get; set; } = string.Empty;
    }

    public class VoidTestRequestHandler : IRequestHandler<VoidTestRequest>
    {
        public Task Handle(VoidTestRequest request, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    [Fact]
    public void Should_register_mediator_and_handlers()
    {
        var services = new ServiceCollection();
        services.AddMediatRSimple<DependencyInjectionTests>();

        var provider = services.BuildServiceProvider();

        // Should register IMediator
        var mediator = provider.GetService<IMediator>();
        mediator.ShouldNotBeNull();
        mediator.ShouldBeOfType<Mediator>();

        // Should register handlers
        var requestHandler = provider.GetService<IRequestHandler<TestRequest, string>>();
        requestHandler.ShouldNotBeNull();
        requestHandler.ShouldBeOfType<TestRequestHandler>();

        var voidRequestHandler = provider.GetService<IRequestHandler<VoidTestRequest>>();
        voidRequestHandler.ShouldNotBeNull();
        voidRequestHandler.ShouldBeOfType<VoidTestRequestHandler>();
    }

    [Fact]
    public void Should_register_handlers_from_multiple_assemblies()
    {
        var services = new ServiceCollection();
        services.AddMediatRSimple(
            typeof(DependencyInjectionTests).Assembly,
            typeof(string).Assembly // System assembly (won't have handlers but should not cause issues)
        );

        var provider = services.BuildServiceProvider();
        var mediator = provider.GetService<IMediator>();
        mediator.ShouldNotBeNull();
    }

    [Fact]
    public void Should_register_handlers_using_type_marker()
    {
        var services = new ServiceCollection();
        services.AddMediatRSimple(typeof(DependencyInjectionTests));

        var serviceDescriptors = services.ToList();

        // Should have IMediator registration
        serviceDescriptors.ShouldContain(s => s.ServiceType == typeof(IMediator));
        
        // Should have handler registrations
        serviceDescriptors.ShouldContain(s => s.ServiceType == typeof(IRequestHandler<TestRequest, string>));
        serviceDescriptors.ShouldContain(s => s.ServiceType == typeof(IRequestHandler<VoidTestRequest>));
    }

    [Fact]
    public void Should_register_with_correct_lifetime()
    {
        var services = new ServiceCollection();
        services.AddMediatRSimple<DependencyInjectionTests>();

        var serviceDescriptors = services.ToList();

        // IMediator should be transient
        var mediatorService = serviceDescriptors.First(s => s.ServiceType == typeof(IMediator));
        mediatorService.Lifetime.ShouldBe(ServiceLifetime.Transient);

        // Handlers should be transient
        var handlerService = serviceDescriptors.First(s => s.ServiceType == typeof(IRequestHandler<TestRequest, string>));
        handlerService.Lifetime.ShouldBe(ServiceLifetime.Transient);
    }

    [Fact]
    public async Task Should_work_with_scoped_services()
    {
        var services = new ServiceCollection();
        services.AddMediatRSimple<DependencyInjectionTests>();
        services.AddScoped<ScopedService>();

        var provider = services.BuildServiceProvider();

        using var scope = provider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var request = new ScopedRequest();
        
        var result = await mediator.Send(request);
        result.ShouldBe("Scoped service used");
    }

    public class ScopedService
    {
        public string GetValue() => "Scoped service used";
    }

    public class ScopedRequest : IRequest<string>
    {
    }

    public class ScopedRequestHandler : IRequestHandler<ScopedRequest, string>
    {
        private readonly ScopedService _scopedService;

        public ScopedRequestHandler(ScopedService scopedService)
        {
            _scopedService = scopedService;
        }

        public Task<string> Handle(ScopedRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_scopedService.GetValue());
        }
    }
}
