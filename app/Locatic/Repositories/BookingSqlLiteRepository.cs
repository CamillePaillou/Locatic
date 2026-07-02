using Locatic.Data;
using Locatic.Interfaces;
using Locatic.Models;
using Microsoft.EntityFrameworkCore;

namespace Locatic.Repositories
{
    public class BookingSqlLiteRepository : IBookingRepository
    {
        private readonly AppDbContext _context;

        public BookingSqlLiteRepository(AppDbContext context)
        {
            _context = context;
        }

        public List<Booking> GetAll()
        {
            return _context.Bookings
                .Include(b => b.Car)
                    .ThenInclude(c => c.Model)
                        .ThenInclude(m => m.Brand)
                .Include(b => b.Client)
                .OrderBy(b => b.StartDate)
                .ToList();
        }

        public Booking? GetById(int id)
        {
            return _context.Bookings
                .Include(b => b.Car)
                    .ThenInclude(c => c.Model)
                        .ThenInclude(m => m.Brand)
                .Include(b => b.Client)
                .FirstOrDefault(b => b.Id == id);
        }

        public void Add(Booking booking)
        {
            _context.Bookings.Add(booking);
            _context.SaveChanges();
        }

        public void Update(Booking booking)
        {
            _context.Bookings.Update(booking);
            _context.SaveChanges();
        }

        public void Delete(Booking booking)
        {
            _context.Bookings.Remove(booking);
            _context.SaveChanges();
        }

        // Deux périodes se chevauchent si : startDate <= existingEnd ET existingStart <= endDate
        public bool IsCarAvailable(int carId, DateOnly startDate, DateOnly endDate, int? excludeBookingId = null)
        {
            return !_context.Bookings.Any(b =>
                b.CarId == carId &&
                (excludeBookingId == null || b.Id != excludeBookingId) &&
                b.StartDate <= endDate &&
                b.EndDate >= startDate);
        }
    }
}
