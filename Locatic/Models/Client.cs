namespace Locatic.Models
{
    public class Client
    {
        public int Id { get; set; }
        public required string LastName { get; set; }
        public required string FirstName { get; set; }
        public required string Email { get; set; }
        public string? PhoneNumber { get; set; }

        public ICollection<Booking> Bookings { get; set; } = [];
    }
}
