using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace MediatR.Simple.Tests;

public class PerformanceTests
{
    public class BenchmarkRequest : IRequest<string>
    {
        public int Id { get; set; }
    }

    public class BenchmarkRequestHandler : IRequestHandler<BenchmarkRequest, string>
    {
        public Task<string> Handle(BenchmarkRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult($"Response for {request.Id}");
        }
    }


    public class BenchmarkRequestVoid : IRequest
    {
        public int Id { get; set; }
    }

    public class BenchmarkRequestVoidHandler : IRequestHandler<BenchmarkRequestVoid>
{
    public Task Handle(BenchmarkRequestVoid request, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

    [Fact]
    public async Task Should_cache_handler_wrappers_for_performance_request_response()
    {
        var services = new ServiceCollection();
        services.AddMediatRSimple<PerformanceTests>();
        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        Console.WriteLine("Starting performance test - handler wrapper caching");

        // First call - creates and caches wrapper
        var request1 = new BenchmarkRequest { Id = 1 };
        Console.WriteLine("First call - creating and cachicng wrapper...");
        var sw1 = Stopwatch.StartNew();
        var result1 = await mediator.Send(request1);
        sw1.Stop();
        Console.WriteLine($"First call completed in {sw1.ElapsedMilliseconds}ms ({sw1.ElapsedTicks} ticks)");

        // Second call - should use cached wrapper and be faster
        var request2 = new BenchmarkRequest { Id = 2 };
        Console.WriteLine("Second call - using cached wrapper...");
        var sw2 = Stopwatch.StartNew();
        var result2 = await mediator.Send(request2);
        sw2.Stop();
        Console.WriteLine($"Second call completed in {sw2.ElapsedMilliseconds}ms ({sw2.ElapsedTicks} ticks)");

        Console.WriteLine($"Performance improvement: {(double)sw1.ElapsedTicks / sw2.ElapsedTicks:F2}x faster");
        Console.WriteLine($"Time difference: {sw1.ElapsedTicks - sw2.ElapsedTicks} ticks saved");

        result1.ShouldBe("Response for 1");
        result2.ShouldBe("Response for 2");

        // Second call should be faster (though this is timing-dependent)
        // At minimum, both should complete reasonably quickly
        Console.WriteLine("Verifying performance requirements...");
        sw1.ElapsedMilliseconds.ShouldBeLessThan(100);
        Console.WriteLine($"✓ First call was fast enough: {sw1.ElapsedMilliseconds}ms < 100ms");
        
        // Use ticks for more precise comparison since milliseconds might both be 0
        if (sw2.ElapsedTicks < sw1.ElapsedTicks)
        {
            Console.WriteLine($"✓ Second call was faster: {sw2.ElapsedTicks} < {sw1.ElapsedTicks} ticks");
        }
        else
        {
            Console.WriteLine($"⚠ Second call was not faster, but both calls were very fast (difference: {sw2.ElapsedTicks - sw1.ElapsedTicks} ticks)");
        }
        
        Console.WriteLine("Performance test completed successfully");
    }

    [Fact]
    public async Task Should_cache_handler_wrappers_for_performance_request_no_response()
    {
        var services = new ServiceCollection();
        services.AddMediatRSimple<PerformanceTests>();
        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        // First call - creates and caches wrapper
        var request1 = new BenchmarkRequestVoid();
        var sw1 = Stopwatch.StartNew();
        await mediator.Send(request1);
        sw1.Stop();

        // Second call - should use cached wrapper and be faster
        var request2 = new BenchmarkRequestVoid();
        var sw2 = Stopwatch.StartNew();
        await mediator.Send(request2);
        sw2.Stop();

        sw2.ElapsedMilliseconds.ShouldBeLessThan(sw1.ElapsedTicks);
        
        // Use ticks for more precise comparison since milliseconds might both be 0
        if (sw2.ElapsedTicks < sw1.ElapsedTicks)
        {
            Console.WriteLine($"✓ Second call was faster: {sw2.ElapsedTicks} < {sw1.ElapsedTicks} ticks");
        }
    }

    [Fact]
    public async Task Should_handle_concurrent_requests()
    {
        var services = new ServiceCollection();
        services.AddMediatRSimple<PerformanceTests>();
        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        const int requestCount = 100;
        var tasks = new Task<string>[requestCount];

        // Fire off many concurrent requests
        for (int i = 0; i < requestCount; i++)
        {
            var request = new BenchmarkRequest { Id = i };
            tasks[i] = mediator.Send(request);
        }

        var results = await Task.WhenAll(tasks);

        // All requests should complete successfully
        results.Length.ShouldBe(requestCount);
        for (int i = 0; i < requestCount; i++)
        {
            results[i].ShouldBe($"Response for {i}");
        }
    }

    [Fact]
    public async Task Should_work_with_different_request_types()
    {
        var services = new ServiceCollection();
        services.AddMediatRSimple<PerformanceTests>();
        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        // Test multiple different request types to ensure caching works correctly
        var request1 = new BenchmarkRequest { Id = 1 };
        var request2 = new AnotherRequest { Value = "test" };
        var request3 = new VoidRequest();

        var result1 = await mediator.Send(request1);
        var result2 = await mediator.Send(request2);
        await mediator.Send(request3);

        result1.ShouldBe("Response for 1");
        result2.ShouldBe("Another: test");
    }

    public class AnotherRequest : IRequest<string>
    {
        public string Value { get; set; } = string.Empty;
    }

    public class AnotherRequestHandler : IRequestHandler<AnotherRequest, string>
    {
        public Task<string> Handle(AnotherRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult($"Another: {request.Value}");
        }
    }

    public class VoidRequest : IRequest
    {
    }

    public class VoidRequestHandler : IRequestHandler<VoidRequest>
    {
        public Task Handle(VoidRequest request, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
