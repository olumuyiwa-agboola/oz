using System;

namespace Oz.Gateway.Handlers;

public class EndpointExecutor : IEndpointExecutor
{
    public Task<IResult> Execute(HttpRequest httpRequest, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
