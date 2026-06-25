using Locatic.Interfaces;
using Locatic.Models;
using Locatic.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Locatic.Controllers
{
    public class CarBrandController : Controller
    {
        private readonly ILogger<CarBrandController> _logger;
        private readonly ICarBrandRepository _repo;

        private List<CarBrand> brands => _repo.GetAll();

        public CarBrandController(ILogger<CarBrandController> logger, ICarBrandRepository repo)
        {
            _logger = logger;
            _repo = repo;
        }

        public IActionResult Index()
        {
            return View(brands);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(CarBrandCreateVM vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            CarBrand brand = new CarBrand
            {
                Name = vm.Name,
                CountryOfOrigin = vm.CountryOfOrigin
            };

            _repo.Add(brand);
            _logger.Log(LogLevel.Debug, brand.Name + " created");

            return RedirectToAction("Index");
        }
    }
}
