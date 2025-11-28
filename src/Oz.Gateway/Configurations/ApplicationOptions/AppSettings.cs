using Oz.Utils;
using FluentValidation;

namespace Oz.Gateway.Configurations.ApplicationOptions;

internal class AppSettings
{
    public Service[] Services { get; set; } = [];
}

internal class AppSettingsValidator : AbstractValidator<AppSettings>
{
    public AppSettingsValidator()
    {
        RuleForEach(x => x.Services)
        .SetValidator(new ServiceValidator());
    }
}

internal class Service
{
    public string Name { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public Endpoint[] Endpoints { get; set; } = [];
}

internal class ServiceValidator : AbstractValidator<Service>
{
    public ServiceValidator()
    {
        RuleFor(x => x.Name)
        .IsRequired();

        RuleFor(x => x.BaseUrl)
        .IsRequired();

        RuleForEach(x => x.Endpoints)
        .SetValidator(new EndpointValidator());
    }
}

internal class Endpoint
{
    public int Version { get; set; }
    public string Method { get; set; } = string.Empty;
    public string UpstreamPathTemplate { get; set; } = string.Empty;
    public string DownstreamPathTemplate { get; set; } = string.Empty;
}

internal class EndpointValidator : AbstractValidator<Endpoint>
{
    public EndpointValidator()
    {
        RuleFor(x => x.Method)
        .IsRequired()
        .MustBeAValidHttpMethod();

        RuleFor(x => x.UpstreamPathTemplate)
        .IsRequired();

        RuleFor(x => x.DownstreamPathTemplate)
        .IsRequired();
    }
}
