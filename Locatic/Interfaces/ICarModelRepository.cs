using Locatic.Models;

namespace Locatic.Interfaces
{
    public interface ICarModelRepository
    {
        List<CarModel> GetAll();
        void Add(CarModel model);
    }
}
