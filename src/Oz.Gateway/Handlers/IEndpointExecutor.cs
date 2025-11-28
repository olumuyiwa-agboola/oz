namespace Oz.Gateway.Handlers;

internal interface IEndpointExecutor
{
    Task<IResult> Execute(HttpRequest httpRequest, CancellationToken cancellationToken);
}
