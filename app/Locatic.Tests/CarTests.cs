using Locatic.Enums;
using Locatic.Models;
using Xunit;

namespace Locatic.Tests;

// Cette classe ne touche pas à la base de données : on teste juste la logique
// métier posée directement dans le setter de Car.DayRate (Models/Car.cs).
public class CarTests
{
    // [Theory] + [InlineData] : xUnit exécute la même méthode une fois par ligne
    // InlineData, avec la valeur en paramètre. Évite de dupliquer 2 [Fact] identiques.
    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void DayRate_Setter_ThrowsArgumentException_WhenNotPositive(decimal invalidRate)
    {
        // Arrange : on construit un objet valide au départ...
        var car = new Car { Registration = "AA-000-AA", Year = 2024, NbSeats = 5, Fuel = Fuel.Petrol, DayRate = 10 };

        // Act + Assert : Assert.Throws exécute le code du lambda et vérifie
        // qu'il lève bien l'exception attendue (sinon le test échoue).
        Assert.Throws<ArgumentException>(() => car.DayRate = invalidRate);
    }

    // [Fact] : test simple, sans paramètre, un seul cas à vérifier.
    [Fact]
    public void DayRate_Setter_AcceptsPositiveValue()
    {
        var car = new Car { Registration = "AA-000-AA", Year = 2024, NbSeats = 5, Fuel = Fuel.Petrol, DayRate = 10 };

        car.DayRate = 42.5m;

        Assert.Equal(42.5m, car.DayRate);
    }
}
