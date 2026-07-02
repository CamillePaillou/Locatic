using System.ComponentModel.DataAnnotations;
using Locatic.Enums;
using Locatic.Models;

namespace Locatic.Models.ViewModels
{
    public class CarEditVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "L'immatriculation est requise")]
        [StringLength(20)]
        public string Registration { get; set; } = null!;

        [Required(ErrorMessage = "L'année est requise")]
        [Range(1960, 2030, ErrorMessage = "Année invalide")]
        public int Year { get; set; }

        [Required(ErrorMessage = "Le tarif journalier est requis")]
        [Range(0.01, 9999, ErrorMessage = "Le tarif doit être positif")]
        public decimal DayRate { get; set; }

        [Required(ErrorMessage = "Le nombre de places est requis")]
        [Range(1, 15, ErrorMessage = "Entre 1 et 15 places")]
        public int NbSeats { get; set; }

        [Required(ErrorMessage = "Le carburant est requis")]
        public Fuel Fuel { get; set; }

        [Required(ErrorMessage = "Le modèle est requis")]
        public int CarModelId { get; set; }

        public List<CarModel> Models { get; set; } = [];
    }
}
