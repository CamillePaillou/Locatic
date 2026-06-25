namespace Locatic.Models
{
    public class CarModel
    {
        public int Id { get; set; }
        public required string Name { get; set; }

        public int CarBrandId { get; set; }
        public CarBrand Brand { get; set; } = null!;

        public ICollection<Car> Cars { get; set; } = [];
    }
}
