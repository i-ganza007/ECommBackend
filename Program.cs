using ECommBackend.DatabaseConns;
using ECommBackend.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddDbContext<SQLiteConn>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("ECommSQLite"));
});
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


app.Run();
