namespace Locatic.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }

        public int CarId { get; set; }
        public Car Car { get; set; } = null!;

        public int ClientId { get; set; }
        public Client Client { get; set; } = null!;
    }
}
