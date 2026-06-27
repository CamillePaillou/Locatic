using Locatic.Data;
using Locatic.Interfaces;
using Locatic.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<ICarBrandRepository, CarBrandSqlLiteRepository>();
builder.Services.AddScoped<ICarModelRepository, CarModelSqlLiteRepository>();
builder.Services.AddScoped<ICarRepository, CarSqlLiteRepository>();
builder.Services.AddScoped<IClientRepository, ClientSqlLiteRepository>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
