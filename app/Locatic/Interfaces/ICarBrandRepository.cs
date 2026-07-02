using Locatic.Models;

namespace Locatic.Interfaces
{
    public interface ICarBrandRepository
    {
        List<CarBrand> GetAll();
        void Add(CarBrand brand);
    }
}
