using ConsumerProject.Services;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults("order-consumer");

MailKit.Telemetry.SmtpClient.Configure();
builder.Services.ConfigureOpenTelemetryTracerProvider(tracing =>
    tracing.AddSource(MailKit.Telemetry.SmtpClient.ActivitySourceName));
builder.Services.ConfigureOpenTelemetryMeterProvider(metrics =>
    metrics.AddMeter(MailKit.Telemetry.SmtpClient.MeterName));

builder.Host.UseSerilog((context, services, loggerConfig) => loggerConfig
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services),
    writeToProviders: true);

builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("Smtp"));
builder.Services.AddOptions<ServiceBusSettings>()
    .Bind(builder.Configuration.GetSection("SecretKeys"))
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddSingleton<EmailSender>();
builder.Services.AddHostedService<ConsumerBackground>();

var app = builder.Build();
app.MapDefaultEndpoints();

app.Run();
