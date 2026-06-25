using Locatic.Enums;

namespace Locatic.Models
{
    public class Car
    {
        private decimal _dayRate;

        public int Id { get; set; }
        public required string Registration { get; set; }
        public int Year { get; set; }

        public decimal DayRate
        {
            get => _dayRate;
            set
            {
                if (value <= 0)
                    throw new ArgumentException("Invalid DayRate");
                _dayRate = value;
            }
        }

        public int NbSeats { get; set; }
        public Fuel Fuel { get; set; }

        public int CarModelId { get; set; }
        public CarModel Model { get; set; } = null!;
    }
}
