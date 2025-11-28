using Asp.Versioning;
using Oz.Gateway.Handlers;
using Microsoft.AspNetCore.Mvc;
using Oz.Gateway.Configurations.ApplicationOptions;

namespace Oz.Gateway.Configurations;

internal static class RequestPipelineConfigurator
{
    internal static void ConfigureRequestPipeline(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider.GetRequiredService<AppSettings>().Services;

        var distinctVersions = services.Select(service => service.Endpoints).SelectMany(endpoint => endpoint)
                        .Select(endpoint => endpoint.Version).Distinct().ToArray();

        foreach (var version in distinctVersions)
        {
            var apiVersionSet = app.NewApiVersionSet()
                    .HasApiVersion(new ApiVersion(version))
                    .Build();

            var versionedApi = app.MapGroup("v{version:apiVersion}")
                .WithApiVersionSet(apiVersionSet);

            var configuredEndpoints = services.SelectMany(service => service.Endpoints, (service, endpoint) => new { service, endpoint })
                    .Where(result => result.endpoint.Version == version).Select(result => (result.service.Name, result.endpoint)).ToArray();

            foreach (var (serviceName, endpoint) in configuredEndpoints)
            {
                versionedApi.MapMethods(endpoint.UpstreamPathTemplate, [endpoint.Method], async (CancellationToken cancellationToken, HttpRequest httpRequest, [FromServices] IEndpointExecutor endpointExecutor)
                    => await endpointExecutor.Execute(httpRequest, cancellationToken))
                    .MapToApiVersion(version)
                    .WithTags(serviceName);
            }
        }

        app.UseSwagger();
        app.UseSwaggerUI(
            options =>
            {
                var descriptions = app.DescribeApiVersions();
                foreach (var desc in descriptions)
                {
                    options.SwaggerEndpoint($"../swagger/{desc.GroupName}/swagger.json", desc.GroupName);
                }
            });
    }
}
