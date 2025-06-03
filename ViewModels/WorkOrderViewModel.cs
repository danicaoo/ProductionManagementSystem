using System.ComponentModel.DataAnnotations;
using ProductionManagementSystem.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductionManagementSystem.ViewModels
{
    public class WorkOrderViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Продукт обязателен")]
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;

        public int? ProductionLineId { get; set; }
        public string? ProductionLineName { get; set; }

        [Required(ErrorMessage = "Количество обязательно")]
        [Range(1, int.MaxValue, ErrorMessage = "Количество должно быть положительным")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Дата начала обязательна")]
        public DateTime StartDate { get; set; } = DateTime.Now;

        public DateTime EstimatedEndDate { get; set; }

        [Required(ErrorMessage = "Статус обязателен")]
        public string Status { get; set; } = string.Empty;

        [Range(0, 100, ErrorMessage = "Прогресс должен быть от 0 до 100")]
        public decimal Progress { get; set; }

        public string? Notes { get; set; }

        public List<MaterialRequirementViewModel>? RequiredMaterials { get; set; }




        public WorkOrderViewModel() { }
        public WorkOrderViewModel(WorkOrder order)
        {
            Id = order.Id;
            ProductId = order.ProductId;
            ProductionLineId = order.ProductionLineId;
            Quantity = order.Quantity;

            StartDate = order.StartDate;
            EstimatedEndDate = order.EstimatedEndDate;
            Status = order.Status ?? string.Empty;
            Progress = order.Progress;
            Notes = order.Notes;
            TotalMinutesRequired = order.TotalMinutesRequired;
            ActualStartDate = order.ActualStartDate;
            ActualEndDate = order.ActualEndDate;

            ProductName = order.Product?.Name ?? "";
            ProductionLineName = order.ProductionLine?.Name ?? "";
        }


        public WorkOrder ToWorkOrder()
        {
            return new WorkOrder
            {
                Id = Id,
                ProductId = ProductId,
                ProductionLineId = ProductionLineId,
                Quantity = Quantity,
                StartDate = StartDate,
                EstimatedEndDate = EstimatedEndDate,
                Status = Status,
                Progress = Progress,
                Notes = Notes
            };
        }

        public int TotalMinutesRequired { get; set; }
        public int MinutesCompleted => ActualStartDate.HasValue && TotalMinutesRequired > 0
            ? Math.Min((int)(DateTime.Now - ActualStartDate.Value).TotalMinutes, TotalMinutesRequired)
            : 0;

        [NotMapped]
        public int ProgressPercentage => TotalMinutesRequired > 0
            ? (int)((double)MinutesCompleted / TotalMinutesRequired * 100)
            : 0;

        public DateTime? ActualStartDate { get; set; }
        public DateTime? ActualEndDate { get; set; }

        public class MaterialRequirementViewModel
        {
            public int MaterialId { get; set; }
            public string? MaterialName { get; set; }
            public decimal RequiredQuantity { get; set; }
            public decimal AvailableQuantity { get; set; }
            public string? UnitOfMeasure { get; set; }
            public bool IsSufficient { get; set; }
        }
    }
}