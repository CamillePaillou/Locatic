using System.ComponentModel.DataAnnotations;

namespace Locatic.Models.ViewModels
{
    public class CarBrandCreateVM
    {
        [Required(ErrorMessage = "Le nom est requis")]
        [StringLength(50)]
        public string Name { get; set; } = null!;

        [StringLength(50)]
        public string? CountryOfOrigin { get; set; }
    }
}
