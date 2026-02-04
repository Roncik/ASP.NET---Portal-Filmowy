using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PortalFilmowy.Models
{
    // Własny atrybut walidacji [cite: 1]
    public class NotFutureDateAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is DateTime date && date > DateTime.Now)
                return new ValidationResult("Data premiery nie może być w przyszłości.");
            return ValidationResult.Success;
        }
    }

    public class Movie
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tytuł jest wymagany")]
        [StringLength(100, MinimumLength = 2)]
        [Display(Name = "Tytuł Filmu")]
        public string Title { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        [Display(Name = "Data Premiery")]
        [NotFutureDate]
        public DateTime ReleaseDate { get; set; }

        [Display(Name = "Reżyser")]
        public string Director { get; set; } = string.Empty;

        // Relacja do Genre (N:1)
        // Więzy integralności danych - Movie musi mieć przypisane Genre
        [Display(Name = "Gatunek")]
        public int GenreId { get; set; }

        [ForeignKey("GenreId")]
        public virtual Genre? Genre { get; set; }

        // Relacja do Reviews (1:N)
        public virtual ICollection<Review>? Reviews { get; set; }
    }
}