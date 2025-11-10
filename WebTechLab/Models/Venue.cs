using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace WebTechLab.Models
{
    public class Venue
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;// Наприклад: "Палац Спорту"

        public string Address { get; set; } = string.Empty;// Наприклад: "пл. Спортивна, 1"

        // Ось дані для карти (C8)
        public double Latitude { get; set; } // Широта
        public double Longitude { get; set; } // Довгота

        // Зв'язок: В одному місці може проходити багато подій
        public virtual ICollection<Event>? Events { get; set; }
    }
}