using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IsolateProject.Types
{

    /// <summary>
    /// Handler interface for requests without response
    /// </summary>
    /// <typeparam name="TRequest">Request type</typeparam>
    public interface IRequestHandler<in TRequest> where TRequest : IRequest
    {
        Task Handle(TRequest request, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Handler interface for requests with response
    /// </summary>
    /// <typeparam name="TRequest">Request type</typeparam>
    /// <typeparam name="TResponse">Response type</typeparam>
    public interface IRequestHandler<in TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken = default);
    }
}
