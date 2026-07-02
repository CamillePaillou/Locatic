using Locatic.Data;
using Locatic.Interfaces;
using Locatic.Models;
using Microsoft.EntityFrameworkCore;

namespace Locatic.Repositories
{
    public class CarModelSqlLiteRepository : ICarModelRepository
    {
        private readonly AppDbContext _context;

        public CarModelSqlLiteRepository(AppDbContext context)
        {
            _context = context;
        }

        public List<CarModel> GetAll()
        {
            return _context.CarModels
                .Include(m => m.Brand)
                .ToList();
        }

        public void Add(CarModel model)
        {
            _context.CarModels.Add(model);
            _context.SaveChanges();
        }
    }
}
