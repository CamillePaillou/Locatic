using System.ComponentModel.DataAnnotations;

namespace Locatic.Models.ViewModels
{
    public class ClientEditVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Le nom est requis")]
        [StringLength(100)]
        public string LastName { get; set; } = null!;

        [Required(ErrorMessage = "Le prénom est requis")]
        [StringLength(100)]
        public string FirstName { get; set; } = null!;

        [Required(ErrorMessage = "L'email est requis")]
        [EmailAddress(ErrorMessage = "Email invalide")]
        [StringLength(200)]
        public string Email { get; set; } = null!;

        [Phone(ErrorMessage = "Numéro de téléphone invalide")]
        [StringLength(20)]
        public string? PhoneNumber { get; set; }
    }
}
