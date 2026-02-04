using System.ComponentModel.DataAnnotations;

namespace PortalFilmowy.Models
{
    public class Genre
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Podaj nazwę gatunku")]
        [Display(Name = "Nazwa Gatunku")]
        public string Name { get; set; }

        public virtual ICollection<Movie>? Movies { get; set; }
    }
}