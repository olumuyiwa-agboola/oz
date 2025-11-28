using Asp.Versioning;
using FluentValidation;
using Microsoft.OpenApi;
using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using Oz.Gateway.Configurations.ApplicationOptions;

namespace Oz.Gateway.Configurations;

/// <summary>
/// Provides the <see cref="ConfigureAndBuild"/> method for configuring and building the <see cref="WebApplicationBuilder"/> instance.
/// </summary>
internal static class ApplicationServicesConfigurator
{
    /// <summary>
    /// Configures the application's services for dependency injection and builds the <see cref="WebApplicationBuilder"/> instance afterwards.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    internal static WebApplication ConfigureAndBuild(this WebApplicationBuilder builder)
    {
        builder.ConfigureApplicationOptions();
        builder.ConfigureApiDocumentationAndVersioning();

        return builder.Build();
    }

    /// <summary>
    /// Configures the application options from configuration files, user secrets, environment variables and other sources.
    /// </summary>
    private static void ConfigureApiDocumentationAndVersioning(this WebApplicationBuilder builder)
    {
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
        });

        builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();

        builder.Services.AddApiVersioning(options =>
        {
            options.ReportApiVersions = true;
            options.DefaultApiVersion = new ApiVersion(1);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        }).AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'V";
            options.SubstituteApiVersionInUrl = true;
        });
    }

    /// <summary>
    /// Configures the application options from configuration files, user secrets, environment variables and other sources.
    /// </summary>
    private static void ConfigureApplicationOptions(this WebApplicationBuilder builder)
    {
        builder.Services.AddOptionsWithFluentValidation<AppSettings, AppSettingsValidator>("");
    }

    /// <summary>
    /// Binds and validates the application options from configuration files, user secrets, environment variables and 
    /// other sources using predefined FluentValidation rules to make them available via dependency injection. With this
    /// implementation, the application options provided in sources such as the appsettings.json file are validated when
    /// the application is started before it starts processing requests. This ensures that all required values are provided
    /// and that only valid/accepted values are provided.
    /// </summary>
    /// <param name="services"></param>
    /// <typeparam name="TOptions"></typeparam>
    /// <param name="configurationSection"></param>
    /// <typeparam name="TOptionsValidator"></typeparam>
    /// <returns></returns>
    private static IServiceCollection AddOptionsWithFluentValidation<TOptions, TOptionsValidator>(this IServiceCollection services, string configurationSection) where TOptions : class, new() where TOptionsValidator : AbstractValidator<TOptions>
    {
        services.AddScoped<IValidator<TOptions>, TOptionsValidator>();

        services.AddOptions<TOptions>()
            .BindConfiguration(configurationSection)
            .ValidateOptionsWithFluentValidation()
            .ValidateOnStart();

        services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<TOptions>>().Value);

        return services;
    }

    /// <summary>
    /// Validates a bound instance of an options class against the predefined FluentValidation rules.
    /// </summary>
    /// <param name="builder"></param>
    /// <typeparam name="TOptions"></typeparam>
    /// <returns></returns>
    private static OptionsBuilder<TOptions> ValidateOptionsWithFluentValidation<TOptions>(this OptionsBuilder<TOptions> builder) where TOptions : class
    {
        builder.Services.AddSingleton<IValidateOptions<TOptions>>(
            serviceProvider => new OptionsFluentValidationHandler<TOptions>(
                serviceProvider,
                builder.Name));

        return builder;
    }
}

/// <summary>
/// Configures the Swagger options.
/// </summary>
/// <param name="provider"></param>
file class ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider) : IConfigureNamedOptions<SwaggerGenOptions>
{
    public void Configure(SwaggerGenOptions options)
    {
        foreach (var description in provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(
                description.GroupName,
                new OpenApiInfo()
                {
                    Title = "Oz",
                    Version = $"v{description.ApiVersion}",
                });
        }
    }

    public void Configure(string? name, SwaggerGenOptions options) => Configure(options);
}

/// <summary>
/// Handles the validation of an istance of an options class using FluentValidation.
/// </summary>
/// <param name="name"></param>
/// <param name="serviceProvider"></param>
/// <typeparam name="TOptions"></typeparam>
file class OptionsFluentValidationHandler<TOptions>(IServiceProvider serviceProvider, string? name) : IValidateOptions<TOptions> where TOptions : class
{
    private readonly string? _name = name;

    /// <summary>
    /// Validates an istance of an options class against the associated FluentValidation ruleset.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public ValidateOptionsResult Validate(string? name, TOptions options)
    {
        if (_name != null && _name != name)
        {
            return ValidateOptionsResult.Skip;
        }

        ArgumentNullException.ThrowIfNull(options, nameof(options));

        using var scope = serviceProvider.CreateScope();

        var validator = scope.ServiceProvider.GetRequiredService<IValidator<TOptions>>();

        var result = validator.Validate(options);

        if (result.IsValid)
        {
            return ValidateOptionsResult.Success;
        }

        var type = options.GetType().Name;
        var errors = new List<string>();

        foreach (var error in result.Errors)
        {
            errors.Add($"Validation failed for {type}.{error.PropertyName}: with error: {error.ErrorMessage}");
        }

        return ValidateOptionsResult.Fail(errors);
    }
}