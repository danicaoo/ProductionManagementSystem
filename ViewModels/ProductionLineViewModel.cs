using System.ComponentModel.DataAnnotations;
using ProductionManagementSystem.Models;

namespace ProductionManagementSystem.ViewModels
{
    public class ProductionLineViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Название линии обязательно")]
        [StringLength(100, ErrorMessage = "Название не должно превышать 100 символов")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Статус обязателен")]
        public string Status { get; set; } = "Active";

        [Required(ErrorMessage = "Коэффициент эффективности обязателен")]
        [Range(0.5, 2.0, ErrorMessage = "Коэффициент должен быть между 0.5 и 2.0")]
        public float EfficiencyFactor { get; set; } = 1.0f;

        public float? CurrentWorkOrderId { get; set; }
        public string? CurrentProductName { get; set; }
        public decimal? CurrentProgress { get; set; }

        public ProductionLineViewModel() { }

        public ProductionLineViewModel(ProductionLine line)
        {
            Id = line.Id;
            Name = line.Name;
            Status = line.Status;
            EfficiencyFactor = line.EfficiencyFactor;
            CurrentWorkOrderId = line.CurrentWorkOrderId;
        }

        public ProductionLine ToProductionLine()
        {
            return new ProductionLine
            {
                Id = Id,
                Name = Name,
                Status = Status,
                EfficiencyFactor = EfficiencyFactor,
                CurrentWorkOrderId = CurrentWorkOrderId
            };
        }
    }
}