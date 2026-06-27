using Locatic.Interfaces;
using Locatic.Models;
using Locatic.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Locatic.Controllers
{
    public class ClientController : Controller
    {
        private readonly ILogger<ClientController> _logger;
        private readonly IClientRepository _repo;

        public ClientController(ILogger<ClientController> logger, IClientRepository repo)
        {
            _logger = logger;
            _repo = repo;
        }

        public IActionResult Index()
        {
            return View(_repo.GetAll());
        }

        public IActionResult Details(int id)
        {
            Client? client = _repo.GetById(id);
            if (client == null) return NotFound();
            return View(client);
        }

        public IActionResult Create()
        {
            return View(new ClientCreateVM());
        }

        [HttpPost]
        public IActionResult Create(ClientCreateVM vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            Client client = new Client
            {
                LastName = vm.LastName,
                FirstName = vm.FirstName,
                Email = vm.Email,
                PhoneNumber = vm.PhoneNumber
            };

            _repo.Add(client);
            _logger.Log(LogLevel.Debug, client.LastName + " created");

            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            Client? client = _repo.GetById(id);
            if (client == null) return NotFound();

            var vm = new ClientEditVM
            {
                Id = client.Id,
                LastName = client.LastName,
                FirstName = client.FirstName,
                Email = client.Email,
                PhoneNumber = client.PhoneNumber
            };

            return View(vm);
        }

        [HttpPost]
        public IActionResult Edit(ClientEditVM vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            Client? client = _repo.GetById(vm.Id);
            if (client == null) return NotFound();

            client.LastName = vm.LastName;
            client.FirstName = vm.FirstName;
            client.Email = vm.Email;
            client.PhoneNumber = vm.PhoneNumber;

            _repo.Update(client);
            _logger.Log(LogLevel.Debug, client.LastName + " updated");

            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            Client? client = _repo.GetById(id);
            if (client == null) return NotFound();
            return View(client);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            Client? client = _repo.GetById(id);
            if (client == null) return NotFound();

            _repo.Delete(client);
            _logger.Log(LogLevel.Debug, client.LastName + " deleted");

            return RedirectToAction("Index");
        }
    }
}
