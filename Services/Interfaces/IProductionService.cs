using ProductionManagementSystem.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProductionManagementSystem.Services
{
    public interface IProductionService
    {
        Task<(bool IsAvailable, Dictionary<int, decimal> MissingMaterials)> CheckMaterialsAvailability(int productId, int quantity);
        Task<TimeSpan> CalculateProductionTime(int productId, int quantity, int? productionLineId);
        Task<bool> ReserveMaterials(int productId, int quantity);
        Task<bool> ReleaseMaterials(int productId, int quantity);
        Task UpdateProductionLineStatus(int lineId, string status);
        Task AssignOrderToProductionLine(int orderId, int lineId);
    }

    public interface IMaterialService
    {
        Task<List<Material>> GetMaterialsAsync(bool? lowStock);
        Task<Material?> GetMaterialAsync(int id);
        Task<Material> CreateMaterialAsync(Material material);
        Task UpdateMaterialStockAsync(int id, decimal amount);
    }

    public interface IProductService
    {
        Task<List<Product>> GetProductsAsync(string? category);
        Task<Product?> GetProductWithMaterialsAsync(int id);
        Task<Product> CreateProductAsync(Product product);
    }

    
}