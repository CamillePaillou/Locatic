using Locatic.Data;
using Locatic.Models;
using Locatic.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Locatic.Tests;

// Même principe que BookingSqlLiteRepositoryTests : une base SQLite en mémoire,
// recréée pour chaque test. Ici on vérifie juste que le passe-plat vers EF Core
// fonctionne (Add écrit bien, GetById relit bien la même ligne).
public class ClientSqlLiteRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _context;
    private readonly ClientSqlLiteRepository _repository;

    public ClientSqlLiteRepositoryTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new AppDbContext(options);
        _context.Database.EnsureCreated();
        _repository = new ClientSqlLiteRepository(_context);
    }

    [Fact]
    public void Add_ThenGetById_ReturnsTheSameClient()
    {
        // Arrange : un client qui n'existe pas encore en base.
        var client = new Client
        {
            FirstName = "Camille",
            LastName = "Paillou",
            Email = "camille.paillou@gmail.com",
        };

        // Act : on l'ajoute, puis on va le relire par son Id.
        // EF Core remplit client.Id automatiquement après SaveChanges()
        // (auto-incrément géré par SQLite), pas besoin de le fixer à la main ici.
        _repository.Add(client);
        var found = _repository.GetById(client.Id);

        // Assert : c'est bien le même client qui revient, avec les bonnes valeurs.
        Assert.NotNull(found);
        Assert.Equal(client.Id, found!.Id);
        Assert.Equal("Camille", found.FirstName);
        Assert.Equal("Paillou", found.LastName);
        Assert.Equal("camille.paillou@gmail.com", found.Email);
    }

    [Fact]
    public void GetById_ReturnsNull_WhenClientDoesNotExist()
    {
        var found = _repository.GetById(9999);

        Assert.Null(found);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}
