using Locatic.Data;
using Locatic.Interfaces;
using Locatic.Models;

namespace Locatic.Repositories
{
    public class ClientSqlLiteRepository : IClientRepository
    {
        private readonly AppDbContext _context;

        public ClientSqlLiteRepository(AppDbContext context)
        {
            _context = context;
        }

        public List<Client> GetAll()
        {
            return _context.Clients.ToList();
        }

        public Client? GetById(int id)
        {
            return _context.Clients.FirstOrDefault(c => c.Id == id);
        }

        public void Add(Client client)
        {
            _context.Clients.Add(client);
            _context.SaveChanges();
        }

        public void Update(Client client)
        {
            _context.Clients.Update(client);
            _context.SaveChanges();
        }

        public void Delete(Client client)
        {
            _context.Clients.Remove(client);
            _context.SaveChanges();
        }
    }
}
