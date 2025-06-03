using System.ComponentModel.DataAnnotations;

namespace ProductionManagementSystem.ViewModels
{
    public class ProductMaterialViewModel
    {
        public int ProductId { get; set; }
        public string? ProductName { get; set; }

        public int MaterialId { get; set; }
        public string? MaterialName { get; set; }

        [Display(Name = "Количество необходимо")]
        [Range(typeof(decimal), "0.01", "79228162514264337593543950335")]
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public decimal QuantityNeeded { get; set; }
        
        public string? UnitOfMeasure { get; set; }
    }
}