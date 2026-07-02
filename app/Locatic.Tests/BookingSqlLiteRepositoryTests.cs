using Locatic.Data;
using Locatic.Models;
using Locatic.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Locatic.Tests;

// Ici on teste un repository, donc on a besoin d'une vraie base pour vérifier
// la requête EF Core (pas juste une méthode C# isolée comme dans CarTests).
// On utilise SQLite en mode ":memory:" : une base temporaire, jamais écrite
// sur le disque, qui n'existe que le temps du test.
//
// IDisposable + le constructeur : xUnit crée une NOUVELLE instance de cette
// classe pour CHAQUE test ([Theory]/[Fact]). Le constructeur joue donc le rôle
// de "Arrange" commun, et Dispose() nettoie après chaque test. Résultat : les
// tests ne se marchent jamais dessus, même s'ils tournent dans n'importe quel ordre.
public class BookingSqlLiteRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _context;
    private readonly BookingSqlLiteRepository _repository;

    public BookingSqlLiteRepositoryTests()
    {
        // Une base SQLite ":memory:" est détruite dès que la connexion se ferme.
        // Il faut donc garder cette connexion ouverte pendant toute la durée du
        // test, sinon la base disparaîtrait avant même qu'on l'utilise.
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new AppDbContext(options);

        // Crée les tables à partir du modèle EF Core (les classes dans Models/).
        // Contrairement à "dotnet ef database update", ça ne rejoue pas
        // l'historique des migrations, ça part directement du modèle actuel.
        // Comme le seed (SeedBeginning) est défini via HasData() dans le modèle,
        // il est appliqué automatiquement ici aussi.
        _context.Database.EnsureCreated();
        _repository = new BookingSqlLiteRepository(_context);

        // On vide les réservations posées par le seed pour repartir d'un état
        // connu : sans ça, les dates de test entrent en collision avec les
        // réservations d'exemple (c'est ce qui a fait échouer le premier essai).
        _context.Bookings.RemoveRange(_context.Bookings);
        _context.SaveChanges();

        // Réservation de référence utilisée par tous les tests ci-dessous :
        // voiture 1, du 10 au 15 juillet 2024.
        _context.Bookings.Add(new Booking
        {
            Id = 100,
            CarId = 1,
            ClientId = 1,
            StartDate = new DateOnly(2024, 7, 10),
            EndDate = new DateOnly(2024, 7, 15),
        });
        _context.SaveChanges();
    }

    // Périodes qui ne touchent pas la réservation de référence (10-15 juillet) :
    // la voiture doit être disponible.
    [Theory]
    [InlineData(2024, 7, 16, 2024, 7, 20)] // juste après
    [InlineData(2024, 7, 1, 2024, 7, 9)]   // juste avant
    public void IsCarAvailable_ReturnsTrue_WhenNoOverlap(int y1, int m1, int d1, int y2, int m2, int d2)
    {
        var available = _repository.IsCarAvailable(1, new DateOnly(y1, m1, d1), new DateOnly(y2, m2, d2));

        Assert.True(available);
    }

    // Périodes qui chevauchent la réservation de référence d'une façon ou
    // d'une autre : la voiture doit être indisponible.
    [Theory]
    [InlineData(2024, 7, 12, 2024, 7, 18)] // chevauche la fin
    [InlineData(2024, 7, 5, 2024, 7, 12)]  // chevauche le début
    [InlineData(2024, 7, 11, 2024, 7, 13)] // incluse dedans
    [InlineData(2024, 7, 5, 2024, 7, 20)]  // englobe toute la période
    public void IsCarAvailable_ReturnsFalse_WhenOverlap(int y1, int m1, int d1, int y2, int m2, int d2)
    {
        var available = _repository.IsCarAvailable(1, new DateOnly(y1, m1, d1), new DateOnly(y2, m2, d2));

        Assert.False(available);
    }

    // Cas d'usage réel : modifier une réservation existante sans qu'elle se
    // bloque elle-même. excludeBookingId permet d'ignorer la réservation 100
    // lors de la vérification, donc les mêmes dates redeviennent "disponibles".
    [Fact]
    public void IsCarAvailable_IgnoresExcludedBooking()
    {
        var available = _repository.IsCarAvailable(1, new DateOnly(2024, 7, 10), new DateOnly(2024, 7, 15), excludeBookingId: 100);

        Assert.True(available);
    }

    // Les mêmes dates mais sur une AUTRE voiture (id 2, qui n'a aucune
    // réservation) : la vérification est bien filtrée par voiture.
    [Fact]
    public void IsCarAvailable_IgnoresOtherCars()
    {
        var available = _repository.IsCarAvailable(2, new DateOnly(2024, 7, 10), new DateOnly(2024, 7, 15));

        Assert.True(available);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}
