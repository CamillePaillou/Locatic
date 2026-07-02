using Locatic.Interfaces;
using Locatic.Models;
using Locatic.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Locatic.Controllers
{
    public class BookingController : Controller
    {
        private readonly ILogger<BookingController> _logger;
        private readonly IBookingRepository _repo;
        private readonly ICarRepository _carRepo;
        private readonly IClientRepository _clientRepo;

        public BookingController(ILogger<BookingController> logger, IBookingRepository repo, ICarRepository carRepo, IClientRepository clientRepo)
        {
            _logger = logger;
            _repo = repo;
            _carRepo = carRepo;
            _clientRepo = clientRepo;
        }

        public IActionResult Index()
        {
            return View(_repo.GetAll());
        }

        public IActionResult Details(int id)
        {
            Booking? booking = _repo.GetById(id);
            if (booking == null) return NotFound();
            return View(booking);
        }

        public IActionResult Create()
        {
            var vm = new BookingCreateVM
            {
                Cars = _carRepo.GetAll(),
                Clients = _clientRepo.GetAll(),
                StartDate = DateOnly.FromDateTime(DateTime.Today),
                EndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1))
            };
            return View(vm);
        }

        [HttpPost]
        public IActionResult Create(BookingCreateVM vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Cars = _carRepo.GetAll();
                vm.Clients = _clientRepo.GetAll();
                return View(vm);
            }

            if (!_repo.IsCarAvailable(vm.CarId, vm.StartDate, vm.EndDate))
            {
                ModelState.AddModelError("CarId", "Cette voiture est déjà réservée sur cette période. Veuillez choisir une autre voiture ou des dates différentes.");
                vm.Cars = _carRepo.GetAll();
                vm.Clients = _clientRepo.GetAll();
                return View(vm);
            }

            Booking booking = new Booking
            {
                StartDate = vm.StartDate,
                EndDate = vm.EndDate,
                CarId = vm.CarId,
                ClientId = vm.ClientId
            };

            _repo.Add(booking);
            _logger.Log(LogLevel.Debug, $"Booking {booking.Id} created");

            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            Booking? booking = _repo.GetById(id);
            if (booking == null) return NotFound();

            var vm = new BookingEditVM
            {
                Id = booking.Id,
                StartDate = booking.StartDate,
                EndDate = booking.EndDate,
                CarId = booking.CarId,
                ClientId = booking.ClientId,
                Cars = _carRepo.GetAll(),
                Clients = _clientRepo.GetAll()
            };

            return View(vm);
        }

        [HttpPost]
        public IActionResult Edit(BookingEditVM vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Cars = _carRepo.GetAll();
                vm.Clients = _clientRepo.GetAll();
                return View(vm);
            }

            if (!_repo.IsCarAvailable(vm.CarId, vm.StartDate, vm.EndDate, excludeBookingId: vm.Id))
            {
                ModelState.AddModelError("CarId", "Cette voiture est déjà réservée sur cette période. Veuillez choisir une autre voiture ou des dates différentes.");
                vm.Cars = _carRepo.GetAll();
                vm.Clients = _clientRepo.GetAll();
                return View(vm);
            }

            Booking? booking = _repo.GetById(vm.Id);
            if (booking == null) return NotFound();

            booking.StartDate = vm.StartDate;
            booking.EndDate = vm.EndDate;
            booking.CarId = vm.CarId;
            booking.ClientId = vm.ClientId;

            _repo.Update(booking);
            _logger.Log(LogLevel.Debug, $"Booking {booking.Id} updated");

            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            Booking? booking = _repo.GetById(id);
            if (booking == null) return NotFound();
            return View(booking);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            Booking? booking = _repo.GetById(id);
            if (booking == null) return NotFound();

            _repo.Delete(booking);
            _logger.Log(LogLevel.Debug, $"Booking {id} deleted");

            return RedirectToAction("Index");
        }
    }
}
