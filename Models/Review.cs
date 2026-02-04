using System.ComponentModel.DataAnnotations;

namespace PortalFilmowy.Models
{
    public class Review
    {
        public int Id { get; set; }

        [Range(1, 10)]
        public int Rating { get; set; }

        [StringLength(500)]
        public string Comment { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;

        public int MovieId { get; set; }
        public virtual Movie? Movie { get; set; }
    }
}