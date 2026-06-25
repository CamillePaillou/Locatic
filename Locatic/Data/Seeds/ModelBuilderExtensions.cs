using Locatic.Enums;
using Locatic.Models;
using Microsoft.EntityFrameworkCore;

namespace Locatic.Data.Seeds
{
    public static class ModelBuilderExtensions
    {
        public static void SeedBeginning(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CarBrand>().HasData(
                new CarBrand { Id = 1, Name = "Renault", CountryOfOrigin = "France" },
                new CarBrand { Id = 2, Name = "Peugeot", CountryOfOrigin = "France" },
                new CarBrand { Id = 3, Name = "Volkswagen", CountryOfOrigin = "Allemagne" }
            );

            modelBuilder.Entity<CarModel>().HasData(
                new CarModel { Id = 1, Name = "Clio", CarBrandId = 1 },
                new CarModel { Id = 2, Name = "Megane", CarBrandId = 1 },
                new CarModel { Id = 3, Name = "208", CarBrandId = 2 },
                new CarModel { Id = 4, Name = "Golf", CarBrandId = 3 }
            );

            modelBuilder.Entity<Car>().HasData(
                new Car { Id = 1, Registration = "AB-123-CD", Year = 2021, DayRate = 35, NbSeats = 5, Fuel = Fuel.Petrol, CarModelId = 1 },
                new Car { Id = 2, Registration = "EF-456-GH", Year = 2020, DayRate = 45, NbSeats = 5, Fuel = Fuel.Diesel, CarModelId = 2 },
                new Car { Id = 3, Registration = "IJ-789-KL", Year = 2022, DayRate = 40, NbSeats = 5, Fuel = Fuel.Electric, CarModelId = 3 },
                new Car { Id = 4, Registration = "MN-012-OP", Year = 2019, DayRate = 50, NbSeats = 5, Fuel = Fuel.Diesel, CarModelId = 4 }
            );

            modelBuilder.Entity<Client>().HasData(
                new Client { Id = 1, LastName = "Martin", FirstName = "Sophie", Email = "sophie.martin@email.com", PhoneNumber = "0612345678" },
                new Client { Id = 2, LastName = "Dupont", FirstName = "Pierre", Email = "pierre.dupont@email.com", PhoneNumber = "0698765432" }
            );

            modelBuilder.Entity<Booking>().HasData(
                new Booking { Id = 1, StartDate = new DateOnly(2024, 7, 1), EndDate = new DateOnly(2024, 7, 7), CarId = 1, ClientId = 1 },
                new Booking { Id = 2, StartDate = new DateOnly(2024, 8, 15), EndDate = new DateOnly(2024, 8, 20), CarId = 3, ClientId = 2 }
            );
        }
    }
}
