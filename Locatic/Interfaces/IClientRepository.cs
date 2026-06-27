using Locatic.Models;

namespace Locatic.Interfaces
{
    public interface IClientRepository
    {
        List<Client> GetAll();
        Client? GetById(int id);
        void Add(Client client);
        void Update(Client client);
        void Delete(Client client);
    }
}
