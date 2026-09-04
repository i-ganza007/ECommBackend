using ECommBackend.CustomErrors.ExceptionFilterLayer;
using ECommBackend.DatabaseConns;
using ECommBackend.Models;
using ECommBackend.Repositories;
using ECommBackend.Repositories.RepoInterfaces;
using ECommBackend.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddDbContext<SQLiteConn>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("ECommSQLite"));
});
builder.Services.AddScoped<IProductRepo,ProductRepo>();
builder.Services.AddScoped<IUserRepo, UserRepo>();
builder.Services.AddScoped<IAdminRepo, AdminRepo>();
builder.Services.AddScoped<IOrderRepo, OrderRepo>();
builder.Services.AddScoped<IImageRepo, ImageRepo>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<AdminService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<ImageService>();

//builder.Services.AddDbContext<SQLConn>(options =>
//{
//    options.UseSql(builder.Configuration.GetConnectionString("ECommSQLite"));
//});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapControllers();
app.UseMiddleware<OperationCancelledHandler>();
app.UseMiddleware<GlobalExceptionLayer>();



app.Run();
