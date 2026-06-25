using Locatic.Interfaces;
using Locatic.Models;
using Locatic.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Locatic.Controllers
{
    public class CarModelController : Controller
    {
        private readonly ILogger<CarModelController> _logger;
        private readonly ICarModelRepository _repo;
        private readonly ICarBrandRepository _brandRepo;

        public CarModelController(ILogger<CarModelController> logger, ICarModelRepository repo, ICarBrandRepository brandRepo)
        {
            _logger = logger;
            _repo = repo;
            _brandRepo = brandRepo;
        }

        public IActionResult Index()
        {
            return View(_repo.GetAll());
        }

        public IActionResult Create()
        {
            var vm = new CarModelCreateVM
            {
                Brands = _brandRepo.GetAll()
            };
            return View(vm);
        }

        [HttpPost]
        public IActionResult Create(CarModelCreateVM vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Brands = _brandRepo.GetAll();
                return View(vm);
            }

            CarModel model = new CarModel
            {
                Name = vm.Name,
                CarBrandId = vm.CarBrandId
            };

            _repo.Add(model);
            _logger.Log(LogLevel.Debug, model.Name + " created");

            return RedirectToAction("Index");
        }
    }
}
