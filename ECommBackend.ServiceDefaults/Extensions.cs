using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Microsoft.Extensions.Hosting;

// Shared by the API and the email consumer: OpenTelemetry, health checks, and the OTLP exporter
// that Aspire points at its dashboard through OTEL_EXPORTER_OTLP_ENDPOINT.
public static class Extensions
{
    private const string HealthEndpointPath = "/health";
    private const string AlivenessEndpointPath = "/alive";

    // Every custom ActivitySource in this solution is named ECommBackend.<Area>.
    public const string ActivitySourcePrefix = "ECommBackend";

    public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.ConfigureOpenTelemetry();
        builder.AddDefaultHealthChecks();
        return builder;
    }

    public static TBuilder ConfigureOpenTelemetry<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        // The Azure SDK only emits Service Bus spans when this switch is on.
        AppContext.SetSwitch("Azure.Experimental.EnableActivitySource", true);

        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        builder.Services.AddOpenTelemetry()
            // Service name and instance come from Aspire's OTEL_* variables; this adds which environment sent them.
            .ConfigureResource(resource => resource.AddAttributes(
            [
                new("deployment.environment.name", builder.Environment.EnvironmentName)
            ]))
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    // GC, thread pool, exceptions, plus process CPU and memory on .NET 9+.
                    .AddRuntimeInstrumentation()
                    // Built into ASP.NET Core but not switched on by the instrumentation above.
                    .AddMeter("Microsoft.AspNetCore.Authentication")
                    .AddMeter("Microsoft.AspNetCore.Authorization")
                    .AddMeter("System.Net.NameResolution")
                    // Our own counters and histograms (orders, emails, logins).
                    .AddMeter($"{ActivitySourcePrefix}.*");
            })
            .WithTracing(tracing =>
            {
                tracing.AddSource(builder.Environment.ApplicationName)
                    .AddSource($"{ActivitySourcePrefix}.*")
                    // Only the sender: the receivers poll in a loop and would add an empty trace per poll.
                    // Processing spans come from our own consumer activities instead.
                    .AddSource("Azure.Messaging.ServiceBus.ServiceBusSender")
                    .AddAspNetCoreInstrumentation(options =>
                        // Health probes would otherwise drown out real requests.
                        options.Filter = context =>
                            !context.Request.Path.StartsWithSegments(HealthEndpointPath)
                            && !context.Request.Path.StartsWithSegments(AlivenessEndpointPath))
                    .AddHttpClientInstrumentation();
            });

        builder.AddOpenTelemetryExporters();

        return builder;
    }

    private static TBuilder AddOpenTelemetryExporters<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        // Aspire sets this. Outside the AppHost nothing is exported and the apps still start fine.
        var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

        if (useOtlpExporter)
        {
            builder.Services.AddOpenTelemetry().UseOtlpExporter();
        }

        // In Azure, set this to the Application Insights connection string and the same traces,
        // metrics and logs go there too. Both exporters can run side by side.
        if (!string.IsNullOrWhiteSpace(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
        {
            builder.Services.AddOpenTelemetry().UseAzureMonitor();
        }

        return builder;
    }

    public static TBuilder AddDefaultHealthChecks<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        return builder;
    }

    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        // Kept to Development, as in the Aspire template.
        if (app.Environment.IsDevelopment())
        {
            app.MapHealthChecks(HealthEndpointPath);
            app.MapHealthChecks(AlivenessEndpointPath, new HealthCheckOptions
            {
                Predicate = r => r.Tags.Contains("live")
            });
        }

        return app;
    }
}
