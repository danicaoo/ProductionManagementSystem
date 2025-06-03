using System.ComponentModel.DataAnnotations;
using ProductionManagementSystem.Models;
using System.Text.Json;

namespace ProductionManagementSystem.ViewModels
{
    public class ProductViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Название продукта обязательно")]
        [StringLength(100, ErrorMessage = "Название не должно превышать 100 символов")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Описание не должно превышать 500 символов")]
        public string Description { get; set; } = string.Empty;

        public string SpecificationsJson { get; set; } = "{}";

        [Required(ErrorMessage = "Категория обязательна")]
        [StringLength(50, ErrorMessage = "Категория не должна превышать 50 символов")]
        public string Category { get; set; } = string.Empty;

        [Required(ErrorMessage = "Минимальный запас обязателен")]
        [Range(0, int.MaxValue, ErrorMessage = "Минимальный запас не может быть отрицательным")]
        public int MinimalStock { get; set; }

        [Required(ErrorMessage = "Время производства обязательно")]
        [Range(1, int.MaxValue, ErrorMessage = "Время производства должно быть положительным")]
        public int ProductionTimePerUnit { get; set; }

        public Dictionary<string, string> Specifications
        {
            get => JsonSerializer.Deserialize<Dictionary<string, string>>(SpecificationsJson) ?? new Dictionary<string, string>();
            set => SpecificationsJson = JsonSerializer.Serialize(value);
        }
        public List<ProductMaterialViewModel> ProductMaterials { get; set; } = new List<ProductMaterialViewModel>();

        public ProductViewModel() { }

        public ProductViewModel(Product product)
{
    Id = product.Id;
    Name = product.Name ?? string.Empty;
    Description = product.Description ?? string.Empty;
    SpecificationsJson = product.Specifications;
    Category = product.Category ?? string.Empty;
    MinimalStock = product.MinimalStock;
    ProductionTimePerUnit = product.ProductionTimePerUnit;

    // Заполняем информацию о материалах
    if (product.ProductMaterials != null)
    {
        ProductMaterials = product.ProductMaterials.Select(pm => new ProductMaterialViewModel
        {
            MaterialId = pm.MaterialId,
            MaterialName = pm.Material?.Name ?? "Неизвестный материал",
            QuantityNeeded = pm.QuantityNeeded,
            UnitOfMeasure = pm.Material?.UnitOfMeasure ?? "н/д"
        }).ToList();
    }
    else
    {
        ProductMaterials = new List<ProductMaterialViewModel>();
    }
}

        

        public Product ToProduct()
        {
            return new Product
            {
                Id = Id,
                Name = Name,
                Description = Description,
                Specifications = SpecificationsJson,
                Category = Category,
                MinimalStock = MinimalStock,
                ProductionTimePerUnit = ProductionTimePerUnit
            };
        } 
            
            
            [Display(Name = "Материалы для продукта")]
            public List<int> SelectedMaterialIds { get; set; } = new List<int>();
            
            [Display(Name = "Количество для каждого материала")]
            public Dictionary<int, decimal> MaterialQuantities { get; set; } = new Dictionary<int, decimal>();

            public class ProductMaterialViewModel
        {
            public int MaterialId { get; set; }
            public string? MaterialName { get; set; }
            public decimal QuantityNeeded { get; set; }
            public string? UnitOfMeasure { get; set; }
        }
    }
}