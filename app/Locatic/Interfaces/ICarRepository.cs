using Locatic.Models;

namespace Locatic.Interfaces
{
    public interface ICarRepository
    {
        List<Car> GetAll();
        Car? GetById(int id);
        void Add(Car car);
        void Update(Car car);
        void Delete(Car car);
    }
}
