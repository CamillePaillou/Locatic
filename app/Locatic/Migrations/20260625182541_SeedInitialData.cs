using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Locatic.Migrations
{
    /// <inheritdoc />
    public partial class SeedInitialData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "CarBrands",
                columns: new[] { "Id", "CountryOfOrigin", "Name" },
                values: new object[,]
                {
                    { 1, "France", "Renault" },
                    { 2, "France", "Peugeot" },
                    { 3, "Allemagne", "Volkswagen" }
                });

            migrationBuilder.InsertData(
                table: "Clients",
                columns: new[] { "Id", "Email", "FirstName", "LastName", "PhoneNumber" },
                values: new object[,]
                {
                    { 1, "sophie.martin@email.com", "Sophie", "Martin", "0612345678" },
                    { 2, "pierre.dupont@email.com", "Pierre", "Dupont", "0698765432" }
                });

            migrationBuilder.InsertData(
                table: "CarModels",
                columns: new[] { "Id", "CarBrandId", "Name" },
                values: new object[,]
                {
                    { 1, 1, "Clio" },
                    { 2, 1, "Megane" },
                    { 3, 2, "208" },
                    { 4, 3, "Golf" }
                });

            migrationBuilder.InsertData(
                table: "Cars",
                columns: new[] { "Id", "CarModelId", "DayRate", "Fuel", "NbSeats", "Registration", "Year" },
                values: new object[,]
                {
                    { 1, 1, 35m, 1, 5, "AB-123-CD", 2021 },
                    { 2, 2, 45m, 0, 5, "EF-456-GH", 2020 },
                    { 3, 3, 40m, 2, 5, "IJ-789-KL", 2022 },
                    { 4, 4, 50m, 0, 5, "MN-012-OP", 2019 }
                });

            migrationBuilder.InsertData(
                table: "Bookings",
                columns: new[] { "Id", "CarId", "ClientId", "EndDate", "StartDate" },
                values: new object[,]
                {
                    { 1, 1, 1, new DateOnly(2024, 7, 7), new DateOnly(2024, 7, 1) },
                    { 2, 3, 2, new DateOnly(2024, 8, 20), new DateOnly(2024, 8, 15) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Bookings",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Bookings",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "CarModels",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "CarModels",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Clients",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Clients",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "CarBrands",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "CarModels",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "CarModels",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "CarBrands",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "CarBrands",
                keyColumn: "Id",
                keyValue: 2);
        }
    }
}
