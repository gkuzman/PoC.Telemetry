using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Shared.Messaging;

namespace Shared;

public static class TelemetryExtensions
{
    public static IOpenTelemetryBuilder AddOpenTelemetry(this IServiceCollection services, string serviceName)
    {
        var uri = Environment.GetEnvironmentVariable("CUSTOM_OTEL_EXPORTER_OTLP_ENDPOINT") ?? throw new InvalidOperationException("CUSTOM_OTEL_EXPORTER_OTLP_ENDPOINT environment variable is not set.");
        var openTelemetryBuilder = services.AddOpenTelemetry()
            .UseOtlpExporter(OtlpExportProtocol.HttpProtobuf, new Uri(uri))
            .ConfigureResource(builder => builder.AddService(serviceName))
            .WithTracing(builder =>
            {
                builder
                    .AddSource(serviceName)
                    .AddAspNetCoreInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation();

                builder
                    .AddSource(TraceableRequestExtensions.Source.Name)
                    .AddSource(ServiceBusDiagnosticSettings.Source.Name);
            })
            .WithMetrics()
            .WithLogging();
        AppContext.SetSwitch("Azure.Experimental.EnableActivitySource", true);
        openTelemetryBuilder.WithTracing(builder => builder.AddSource("Azure.Messaging.ServiceBus"));


        return openTelemetryBuilder;
    }
}