using Microsoft.EntityFrameworkCore;
using ProductionManagementSystem.Data;
using ProductionManagementSystem.Models;

namespace ProductionManagementSystem.Services
{
    public class ProductionService : IProductionService
    {
        private readonly ApplicationDbContext _context;

        public ProductionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> CheckMaterialsAvailability(int productId, int quantity)
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
                    return false;
                }
            }

            return true;
        }

        public async Task<TimeSpan> CalculateProductionTime(int productId, int quantity, int? productionLineId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
                throw new ArgumentException("Product not found");

            float efficiency = 1.0f;
            if (productionLineId.HasValue)
            {
                var productionLine = await _context.ProductionLines.FindAsync(productionLineId.Value);
                if (productionLine != null)
                {
                    efficiency = productionLine.EfficiencyFactor;
                }
            }

            var totalMinutes = (quantity * product.ProductionTimePerUnit) / efficiency;
            return TimeSpan.FromMinutes(totalMinutes);
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
                    pm.Material.Quantity -= requiredAmount;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<bool> ReleaseMaterials(int productId, int quantity)
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
                    pm.Material.Quantity += requiredAmount;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task UpdateProductionLineStatus(int lineId, string status)
        {
            var line = await _context.ProductionLines.FindAsync(lineId);
            if (line == null)
                throw new ArgumentException("Production line not found");

            line.Status = status;
            await _context.SaveChangesAsync();
        }

        public async Task AssignOrderToProductionLine(int orderId, int lineId)
        {
            var order = await _context.WorkOrders.FindAsync(orderId);
            var line = await _context.ProductionLines.FindAsync(lineId);

            if (order == null || line == null)
                throw new ArgumentException("Order or production line not found");

            line.CurrentWorkOrderId = orderId;
            order.ProductionLineId = lineId;
            order.Status = "InProgress";

            await _context.SaveChangesAsync();
        }
    }
}