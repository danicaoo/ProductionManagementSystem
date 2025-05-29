using System.ComponentModel.DataAnnotations;
using ProductionManagementSystem.Models;

namespace ProductionManagementSystem.ViewModels
{
    public class MaterialViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Название материала обязательно")]
        [StringLength(100, ErrorMessage = "Название не должно превышать 100 символов")]
        public string Name { get; set; } = string.Empty; 

        [Required(ErrorMessage = "Количество обязательно")]
        [Range(0, double.MaxValue, ErrorMessage = "Количество не может быть отрицательным")]
        public decimal Quantity { get; set; }

        [Required(ErrorMessage = "Единица измерения обязательна")]
        [StringLength(20, ErrorMessage = "Единица измерения не должна превышать 20 символов")]
        public string UnitOfMeasure { get; set; } = string.Empty; 

        [Required(ErrorMessage = "Минимальный запас обязателен")]
        [Range(0, double.MaxValue, ErrorMessage = "Минимальный запас не может быть отрицательным")]
        public decimal MinimalStock { get; set; }

        public bool IsLowStock { get; set; }

       
        public MaterialViewModel() { }

        public MaterialViewModel(Material material)
        {
            if (material == null)
                throw new ArgumentNullException(nameof(material));

            Id = material.Id;
            Name = material.Name ?? string.Empty;
            Quantity = material.Quantity;
            UnitOfMeasure = material.UnitOfMeasure ?? string.Empty;
            MinimalStock = material.MinimalStock;
            IsLowStock = material.IsLowStock;
        }

        public Material ToMaterial()
        {
            return new Material
            {
                Id = Id,
                Name = Name,
                Quantity = Quantity,
                UnitOfMeasure = UnitOfMeasure,
                MinimalStock = MinimalStock
            };
        }
    }
}