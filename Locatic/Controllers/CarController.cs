using Locatic.Enums;
using Locatic.Interfaces;
using Locatic.Models;
using Locatic.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Locatic.Controllers
{
    public class CarController : Controller
    {
        private readonly ILogger<CarController> _logger;
        private readonly ICarRepository _repo;
        private readonly ICarModelRepository _modelRepo;

        public CarController(ILogger<CarController> logger, ICarRepository repo, ICarModelRepository modelRepo)
        {
            _logger = logger;
            _repo = repo;
            _modelRepo = modelRepo;
        }

        public IActionResult Index()
        {
            return View(_repo.GetAll());
        }

        public IActionResult Details(int id)
        {
            Car? car = _repo.GetById(id);
            if (car == null) return NotFound();
            return View(car);
        }

        public IActionResult Create()
        {
            var vm = new CarCreateVM
            {
                Models = _modelRepo.GetAll()
            };
            return View(vm);
        }

        [HttpPost]
        public IActionResult Create(CarCreateVM vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Models = _modelRepo.GetAll();
                return View(vm);
            }

            Car car = new Car
            {
                Registration = vm.Registration,
                Year = vm.Year,
                DayRate = vm.DayRate,
                NbSeats = vm.NbSeats,
                Fuel = vm.Fuel,
                CarModelId = vm.CarModelId
            };

            _repo.Add(car);
            _logger.Log(LogLevel.Debug, car.Registration + " created");

            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            Car? car = _repo.GetById(id);
            if (car == null) return NotFound();

            var vm = new CarEditVM
            {
                Id = car.Id,
                Registration = car.Registration,
                Year = car.Year,
                DayRate = car.DayRate,
                NbSeats = car.NbSeats,
                Fuel = car.Fuel,
                CarModelId = car.CarModelId,
                Models = _modelRepo.GetAll()
            };

            return View(vm);
        }

        [HttpPost]
        public IActionResult Edit(CarEditVM vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Models = _modelRepo.GetAll();
                return View(vm);
            }

            Car? car = _repo.GetById(vm.Id);
            if (car == null) return NotFound();

            car.Registration = vm.Registration;
            car.Year = vm.Year;
            car.DayRate = vm.DayRate;
            car.NbSeats = vm.NbSeats;
            car.Fuel = vm.Fuel;
            car.CarModelId = vm.CarModelId;

            _repo.Update(car);
            _logger.Log(LogLevel.Debug, car.Registration + " updated");

            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            Car? car = _repo.GetById(id);
            if (car == null) return NotFound();
            return View(car);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            Car? car = _repo.GetById(id);
            if (car == null) return NotFound();

            _repo.Delete(car);
            _logger.Log(LogLevel.Debug, car.Registration + " deleted");

            return RedirectToAction("Index");
        }
    }
}
