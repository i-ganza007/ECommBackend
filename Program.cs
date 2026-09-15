using ECommBackend.CustomErrors.ExceptionFilterLayer;
using ECommBackend.DatabaseConns;
using ECommBackend.Models;
using Scalar.AspNetCore;
using ECommBackend.Repositories;
using ECommBackend.Repositories.RepoInterfaces;
using ECommBackend.Services;
using ECommBackend.Services.IJWTServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Scalar renders whatever this document describes, so it has to be generated first.
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
builder.Logging.ClearProviders();
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JWTSettings")); // Maps JWTSettings to the appsettings.json

builder.Host.UseSerilog(); 

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
builder.Services.AddSingleton<IJWTService, JwtServiceAuth>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<ImageService>();
builder.Services.AddScoped<VariantService>();
builder.Services.AddScoped<CategoryService>();

// IMiddleware implementations are resolved from DI per request, so they must be registered.
builder.Services.AddScoped<GlobalExceptionLayer>();
builder.Services.AddScoped<OperationCancelledHandler>();

builder.Services.AddOptions<JwtSettings>().BindConfiguration("JWTSettings");
//builder.Services.AddDbContext<SQLConn>(options =>
//{
//    options.UseSql(builder.Configuration.GetConnectionString("ECommSQLite"));
//});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();

    options.RequireHttpsMetadata = true; // Always require HTTPS in production
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

    // Custom event handlers for detailed logging
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
        }
    };
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

// Outermost, and before MapControllers: middleware added after the endpoint is mapped
// never gets to wrap the controller that threw.
app.UseMiddleware<GlobalExceptionLayer>();
app.UseMiddleware<OperationCancelledHandler>();

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();                 // serves /openapi/v1.json
    app.MapScalarApiReference();      // serves the UI at /scalar
}

app.MapControllers();

app.Run();
