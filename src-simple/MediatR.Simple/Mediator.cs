using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace MediatR.Simple;

/// <summary>
/// Simple mediator implementation for request/response patterns
/// </summary>
public class Mediator : IMediator
{
    private readonly IServiceProvider _serviceProvider;
    private static readonly ConcurrentDictionary<Type, RequestHandlerBase> _requestHandlers = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="Mediator"/> class.
    /// </summary>
    /// <param name="serviceProvider">Service provider. Can be a scoped or root provider</param>
    public Mediator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var handler = (RequestHandlerWrapper<TResponse>)_requestHandlers.GetOrAdd(request.GetType(), static requestType =>
        {
            var wrapperType = typeof(RequestHandlerWrapperImpl<,>).MakeGenericType(requestType, typeof(TResponse));
            var wrapper = Activator.CreateInstance(wrapperType) ?? throw new InvalidOperationException($"Could not create wrapper type for {requestType}");
            return (RequestHandlerBase)wrapper;
        });

        return handler.Handle(request, _serviceProvider, cancellationToken);
    }

    public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IRequest
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var handler = (RequestHandlerWrapper)_requestHandlers.GetOrAdd(request.GetType(), static requestType =>
        {
            var wrapperType = typeof(RequestHandlerWrapperImpl<>).MakeGenericType(requestType);
            var wrapper = Activator.CreateInstance(wrapperType) ?? throw new InvalidOperationException($"Could not create wrapper type for {requestType}");
            return (RequestHandlerBase)wrapper;
        });

        return handler.Handle(request, _serviceProvider, cancellationToken);
    }
}

// Internal wrapper classes to handle the dynamic dispatch
internal abstract class RequestHandlerBase
{
}

internal abstract class RequestHandlerWrapper<TResponse> : RequestHandlerBase
{
    public abstract Task<TResponse> Handle(IRequest<TResponse> request, IServiceProvider serviceProvider, CancellationToken cancellationToken);
}

internal class RequestHandlerWrapperImpl<TRequest, TResponse> : RequestHandlerWrapper<TResponse>
    where TRequest : IRequest<TResponse>
{
    public override Task<TResponse> Handle(IRequest<TResponse> request, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var handler = serviceProvider.GetRequiredService<IRequestHandler<TRequest, TResponse>>();
        return handler.Handle((TRequest) request, cancellationToken);
    }
}



internal abstract class RequestHandlerWrapper : RequestHandlerBase
{
    public abstract Task Handle(IRequest request, IServiceProvider serviceProvider, CancellationToken cancellationToken);
}

internal class RequestHandlerWrapperImpl<TRequest> : RequestHandlerWrapper
    where TRequest : IRequest
{
    public override Task Handle(IRequest request, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var handler = serviceProvider.GetRequiredService<IRequestHandler<TRequest>>();
        return handler.Handle((TRequest) request, cancellationToken);
    }
}
