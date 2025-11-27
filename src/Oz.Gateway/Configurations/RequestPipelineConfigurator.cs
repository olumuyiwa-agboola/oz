namespace Oz.Gateway.Configurations;

public static class RequestPipelineConfigurator
{
    public static void ConfigureRequestPipeline(this WebApplication app)
    {
        app.MapGet("/", () => "Hello World!");
    }
}
