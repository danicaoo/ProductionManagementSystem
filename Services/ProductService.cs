using Microsoft.EntityFrameworkCore;
using ProductionManagementSystem.Data;
using ProductionManagementSystem.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProductionManagementSystem.Services
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;
    private readonly ILogger<ProductService> _logger; 

    public ProductService(
        ApplicationDbContext context, 
        ILogger<ProductService> logger) 
    {
        _context = context;
        _logger = logger;
    }

        public async Task<List<Product>> GetProductsAsync(string? category)
        {
            IQueryable<Product> query = _context.Products;

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(p => p.Category == category);
            }

            return await query.ToListAsync();
        }

        public async Task<Product?> GetProductWithMaterialsAsync(int id)
        {
            return await _context.Products
                .Include(p => p.ProductMaterials)
                .ThenInclude(pm => pm.Material)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Product> CreateProductAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }
        public async Task<(bool IsAvailable, Dictionary<int, decimal> MissingMaterials)> CheckMaterialsAvailability(int productId, int quantity)
    {
        var result = new Dictionary<int, decimal>();
        var isAvailable = true;

        var productMaterials = await _context.ProductMaterials
            .Where(pm => pm.ProductId == productId)
            .Include(pm => pm.Material)
            .ToListAsync();

        foreach (var pm in productMaterials)
        {
            if (pm.Material == null) continue;
            
            var requiredAmount = pm.QuantityNeeded * quantity;
            var availableAmount = pm.Material.Quantity;
            
            if (availableAmount < requiredAmount)
            {
                isAvailable = false;
                var missing = requiredAmount - availableAmount;
                result.Add(pm.MaterialId, missing);
            }
        }

        return (isAvailable, result);
    }

        public async Task<bool> ReserveMaterials(int productId, int quantity)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            var productMaterials = await _context.ProductMaterials
                .Where(pm => pm.ProductId == productId)
                .Include(pm => pm.Material)
                .ToListAsync();

            foreach (var pm in productMaterials)
            {
                if (pm.Material == null) return false;
                
                var requiredAmount = pm.QuantityNeeded * quantity;
                if (pm.Material.Quantity < requiredAmount)
                {
                    await transaction.RollbackAsync();
                    return false;
                }
                
                pm.Material.Quantity -= requiredAmount;
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при резервировании материалов"); 
            await transaction.RollbackAsync();
            return false;
        }
    }
    }
}