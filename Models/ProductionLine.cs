using System.ComponentModel.DataAnnotations;

namespace ProductionManagementSystem.Models
{
    public class ProductionLine
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Название линии обязательно")]
        [StringLength(100, ErrorMessage = "Название не должно превышать 100 символов")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Статус обязателен")]
        public string Status { get; set; } = "Active"; 

        [Required(ErrorMessage = "Коэффициент эффективности обязателен")]
        [Range(0.5f, 2.0f, ErrorMessage = "Коэффициент должен быть между 0.5 и 2.0")]
        public float EfficiencyFactor { get; set; } = 1.0f;

        public float? CurrentWorkOrderId { get; set; }

        public ICollection<WorkOrder> WorkOrders { get; set; } = new List<WorkOrder>();
    }
}