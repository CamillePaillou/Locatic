namespace Locatic.Models
{
    public class CarBrand
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? CountryOfOrigin { get; set; }

        public ICollection<CarModel> Models { get; set; } = [];
    }
}
