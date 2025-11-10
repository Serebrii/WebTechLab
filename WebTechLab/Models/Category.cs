using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace WebTechLab.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;// Наприклад: "Музика", "Технології", "Спорт"

        // Зв'язок: Одна категорія може мати багато подій
        public virtual ICollection<Event>? Events { get; set; }
    }
}