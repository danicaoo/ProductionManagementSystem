using System.ComponentModel.DataAnnotations;

namespace ProductionManagementSystem.Models
{
    public class ProductMaterial
    {
        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public int MaterialId { get; set; }
        public Material? Material { get; set; }

        [Range(typeof(decimal), "0.01", "79228162514264337593543950335")]
        public decimal QuantityNeeded { get; set; }
    }
}