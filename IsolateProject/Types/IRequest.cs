﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IsolateProject.Types
{
    public interface IRequest : IBaseRequest { }

    /// <summary>
    /// Marker interface to represent a request with a response
    /// </summary>
    /// <typeparam name="TResponse">Response type</typeparam>
    public interface IRequest<out TResponse> : IBaseRequest { }

    /// <summary>
    /// Allows for generic type constraints of objects implementing IRequest or IRequest{TResponse}
    /// </summary>
    public interface IBaseRequest { }
}
