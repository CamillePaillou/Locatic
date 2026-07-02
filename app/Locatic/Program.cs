using Locatic.Data;
using Locatic.Interfaces;
using Locatic.Repositories;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>(name: "sqlite", tags: new[] { "ready" });

builder.Services.AddScoped<ICarBrandRepository, CarBrandSqlLiteRepository>();
builder.Services.AddScoped<ICarModelRepository, CarModelSqlLiteRepository>();
builder.Services.AddScoped<ICarRepository, CarSqlLiteRepository>();
builder.Services.AddScoped<IClientRepository, ClientSqlLiteRepository>();
builder.Services.AddScoped<IBookingRepository, BookingSqlLiteRepository>();

var app = builder.Build();

// Applique les migrations en attente au démarrage : indispensable en conteneur,
// où le volume SQLite peut être vide (premier déploiement) et où il n'y a
// personne pour lancer `dotnet ef database update` à la main.
using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// Liveness : le process répond, sans vérifier de dépendance externe.
// Un échec ici doit faire redémarrer le pod (le process est bloqué/planté).
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false,
});

// Readiness : vérifie en plus que la connexion SQLite fonctionne.
// Un échec ici doit juste retirer le pod du service, pas le redémarrer.
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
