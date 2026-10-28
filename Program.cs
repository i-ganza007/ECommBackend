using ECommBackend.CustomErrors.ExceptionFilterLayer;
using ECommBackend.DatabaseConns;
using ECommBackend.Models;

using Scalar.AspNetCore;
using ECommBackend.Repositories;
using ECommBackend.Repositories.RepoInterfaces;
using ECommBackend.Services;
using ECommBackend.Services.IJWTServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info.Title = "ECommBackend API";
        document.Info.Version = "v1";
        document.Info.Description = "Products, variants, orders, users and admins.";
        return Task.CompletedTask;
    });
});

builder.Services.AddDbContext<SQLiteConn>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("ECommSQLite"));
});
builder.Services.AddHealthChecks()
    .AddDbContextCheck<SQLiteConn>("database", tags: ["db"]);
builder.Logging.ClearProviders();

builder.AddServiceDefaults("inventory-api");
builder.Services.ConfigureOpenTelemetryTracerProvider(tracing => tracing.AddEntityFrameworkCoreInstrumentation());
builder.Services.ConfigureOpenTelemetryMeterProvider(metrics => metrics.AddMeter("Microsoft.EntityFrameworkCore"));
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JWTSettings"));
builder.Services.AddOptions<ServiceBusSettings>()
    .Bind(builder.Configuration.GetSection("SecretKeys"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Host.UseSerilog((context, services, loggerConfig) => loggerConfig
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services),
    writeToProviders: true);

builder.Services.AddScoped<IProductRepo,ProductRepo>();
builder.Services.AddScoped<IUserRepo, UserRepo>();
builder.Services.AddScoped<ICategory, CategoriesRepo>();
builder.Services.AddScoped<IAdminRepo, AdminRepo>();
builder.Services.AddScoped<IOrderRepo, OrderRepo>();
builder.Services.AddScoped<IImageRepo, ImageRepo>();
builder.Services.AddScoped<IVariantRepo, VariantRepo>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<AdminService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddSingleton<AzureServiceBusClass>();
builder.Services.AddSingleton<IJWTService, JwtServiceAuth>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<ImageService>();
builder.Services.AddScoped<VariantService>();
builder.Services.AddScoped<CategoryService>();


builder.Services.AddHostedService<AzureBackgroundService>();
builder.Services.AddHostedService<PendingOrdersGaugeService>();

builder.Services.AddScoped<GlobalExceptionLayer>();
builder.Services.AddScoped<OperationCancelledHandler>();

builder.Services.AddOptions<JwtSettings>().BindConfiguration("JWTSettings");
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();

    options.RequireHttpsMetadata = true; 
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings.SecretKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(1)
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            var logger = context.HttpContext.RequestServices.GetService<ILogger<Program>>();
            logger?.LogWarning("JWT authentication failed: {Exception}", context.Exception.Message);
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            var logger = context.HttpContext.RequestServices.GetService<ILogger<Program>>();
            logger?.LogInformation("JWT token validated for user: {User}", context.Principal?.Identity?.Name);
            return Task.CompletedTask;
        },
        OnMessageReceived = context =>
        {
            if (context.HttpContext.Request.Cookies.TryGetValue("access_token",out var jwt_token)) {
                context.Token = jwt_token;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorizationBuilder().AddPolicy("adminUsers", policy => policy.RequireRole(Roles.admin.ToString())).AddPolicy("users", policy => policy.RequireRole(Roles.logged_in.ToString()));
var app = builder.Build();


app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live")
});
app.MapHealthChecks("/health/db", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("db"),
    ResponseWriter = WriteHealthJson
});

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}


app.UseMiddleware<GlobalExceptionLayer>();
app.UseMiddleware<OperationCancelledHandler>();
app.UseSerilogRequestLogging();

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapControllers();
app.MapGet("/version", (IConfiguration configuration) => Results.Ok(new
{
    version = configuration["APP_VERSION"] ?? "unknown"
}));
app.MapDefaultEndpoints();

app.Run();

static Task WriteHealthJson(HttpContext context, HealthReport report)
{
    context.Response.ContentType = "application/json";
    return context.Response.WriteAsJsonAsync(new
    {
        status = report.Status.ToString(),
        totalDurationMs = report.TotalDuration.TotalMilliseconds,
        checks = report.Entries.Select(entry => new
        {
            name = entry.Key,
            status = entry.Value.Status.ToString(),
            durationMs = entry.Value.Duration.TotalMilliseconds,
            description = entry.Value.Description
        })
    });
}
