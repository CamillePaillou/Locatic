using System.ComponentModel.DataAnnotations;
using Locatic.Models;

namespace Locatic.Models.ViewModels
{
    public class CarModelCreateVM
    {
        [Required(ErrorMessage = "Le nom est requis")]
        [StringLength(50)]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "La marque est requise")]
        public int CarBrandId { get; set; }

        public List<CarBrand> Brands { get; set; } = [];
    }
}
