using System.ComponentModel.DataAnnotations;

namespace ProductionManagementSystem.Models
{
    public class Material
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string? Name { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Quantity { get; set; }

        [StringLength(20)]
        public string? UnitOfMeasure { get; set; }

        [Range(0, double.MaxValue)]
        public decimal MinimalStock { get; set; }

        public ICollection<ProductMaterial> ProductMaterials { get; set; } = new List<ProductMaterial>();

        public bool IsLowStock => Quantity < MinimalStock;
    }
}