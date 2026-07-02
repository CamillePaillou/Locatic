using System.ComponentModel.DataAnnotations;
using Locatic.Models;

namespace Locatic.Models.ViewModels
{
    public class BookingCreateVM : IValidatableObject
    {
        [Required(ErrorMessage = "La date de début est requise")]
        public DateOnly StartDate { get; set; }

        [Required(ErrorMessage = "La date de fin est requise")]
        public DateOnly EndDate { get; set; }

        [Required(ErrorMessage = "La voiture est requise")]
        public int CarId { get; set; }

        [Required(ErrorMessage = "Le client est requis")]
        public int ClientId { get; set; }

        public List<Car> Cars { get; set; } = [];
        public List<Client> Clients { get; set; } = [];

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (EndDate <= StartDate)
                yield return new ValidationResult(
                    "La date de fin doit être après la date de début.",
                    new[] { nameof(EndDate) });
        }
    }
}
