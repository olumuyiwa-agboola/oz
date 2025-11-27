namespace Oz.Gateway.Configurations;

public static class ApplicationServicesConfigurator
{
    public static WebApplication ConfigureAndBuild(this WebApplicationBuilder builder)
    {
        return builder.Build();
    }
}
