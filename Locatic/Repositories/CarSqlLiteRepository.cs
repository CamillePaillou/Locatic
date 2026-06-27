using Locatic.Data;
using Locatic.Interfaces;
using Locatic.Models;
using Microsoft.EntityFrameworkCore;

namespace Locatic.Repositories
{
    public class CarSqlLiteRepository : ICarRepository
    {
        private readonly AppDbContext _context;

        public CarSqlLiteRepository(AppDbContext context)
        {
            _context = context;
        }

        public List<Car> GetAll()
        {
            return _context.Cars
                .Include(c => c.Model)
                    .ThenInclude(m => m.Brand)
                .ToList();
        }

        public Car? GetById(int id)
        {
            return _context.Cars
                .Include(c => c.Model)
                    .ThenInclude(m => m.Brand)
                .FirstOrDefault(c => c.Id == id);
        }

        public void Add(Car car)
        {
            _context.Cars.Add(car);
            _context.SaveChanges();
        }

        public void Update(Car car)
        {
            _context.Cars.Update(car);
            _context.SaveChanges();
        }

        public void Delete(Car car)
        {
            _context.Cars.Remove(car);
            _context.SaveChanges();
        }
    }
}
