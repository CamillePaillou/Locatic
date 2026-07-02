using Locatic.Data;
using Locatic.Interfaces;
using Locatic.Models;

namespace Locatic.Repositories
{
    public class CarBrandSqlLiteRepository : ICarBrandRepository
    {
        private readonly AppDbContext _context;

        public CarBrandSqlLiteRepository(AppDbContext context)
        {
            _context = context;
        }

        public List<CarBrand> GetAll()
        {
            return _context.CarBrands.ToList();
        }

        public void Add(CarBrand brand)
        {
            _context.CarBrands.Add(brand);
            _context.SaveChanges();
        }
    }
}
