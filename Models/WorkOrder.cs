using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductionManagementSystem.Models
{
    public class WorkOrder
    {
        public int Id { get; set; }

        [Required]
        [ForeignKey("Product")]
        public int ProductId { get; set; }
        public Product? Product { get; set; }

        [ForeignKey("ProductionLine")]
        public int? ProductionLineId { get; set; }
        public ProductionLine? ProductionLine { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime StartDate { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime EstimatedEndDate { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending"; 

        [Range(0, 100)]
        public decimal Progress { get; set; } = 0;

        public string? Notes { get; set; }
    }
}