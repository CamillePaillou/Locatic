using Locatic.Models;

namespace Locatic.Interfaces
{
    public interface IBookingRepository
    {
        List<Booking> GetAll();
        Booking? GetById(int id);
        void Add(Booking booking);
        void Update(Booking booking);
        void Delete(Booking booking);
        bool IsCarAvailable(int carId, DateOnly startDate, DateOnly endDate, int? excludeBookingId = null);
    }
}
