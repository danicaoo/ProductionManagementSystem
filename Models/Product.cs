using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace ProductionManagementSystem.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string? Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public string Specifications { get; set; } = "{}";

        [StringLength(50)]
        public string? Category { get; set; }

        [Range(0, int.MaxValue)]
        public int MinimalStock { get; set; }

        [Range(1, int.MaxValue)]
        public int ProductionTimePerUnit { get; set; }

         public virtual ICollection<ProductMaterial> ProductMaterials { get; set; } = new List<ProductMaterial>();

        public Dictionary<string, string> GetSpecifications()
        {
            return JsonSerializer.Deserialize<Dictionary<string, string>>(Specifications) ?? new Dictionary<string, string>();
        }

        public void SetSpecifications(Dictionary<string, string> specs)
        {
            Specifications = JsonSerializer.Serialize(specs);
        }
        
    }
}