using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebTechLab.Models
{
    public class Event
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;// Назва події

        // Поле для C12 (Markdown Editor)
        public string? Description { get; set; }

        // Поле для C1 (File field)
        public string? EventPosterUrl { get; set; } // Посилання на картинку-постер

        public DateTime StartTime { get; set; } // Час початку

        //Зв'язки (Foreign Keys)

        // Зв'язок з Категорією
        [Required]
        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }

        // Зв'язок з Місцем проведення
        [Required]
        public int VenueId { get; set; }
        [ForeignKey("VenueId")]
        public virtual Venue? Venue { get; set; }
    }
}