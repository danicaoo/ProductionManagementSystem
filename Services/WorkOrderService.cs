using Microsoft.EntityFrameworkCore;
using ProductionManagementSystem.Data;
using ProductionManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductionManagementSystem.Services
{
    public class WorkOrderService : IWorkOrderService
    {
        private readonly ApplicationDbContext _context;
    private readonly IProductionService _productionService;
    private readonly ILogger<WorkOrderService> _logger; 

    public WorkOrderService(
        ApplicationDbContext context, 
        IProductionService productionService,
        ILogger<WorkOrderService> logger)
    {
        _context = context;
        _productionService = productionService;
        _logger = logger;
    }

    public async Task<(WorkOrder? workOrder, Dictionary<int, decimal>? missingMaterials)> 
        CreateWorkOrderAsync(WorkOrder workOrder)
    {
        var availabilityResult = await _productionService.CheckMaterialsAvailability(
            workOrder.ProductId, workOrder.Quantity);
            
        var product = await _context.Products.FindAsync(workOrder.ProductId);
        var productionLine = workOrder.ProductionLineId.HasValue
            ? await _context.ProductionLines.FindAsync(workOrder.ProductionLineId.Value)
            : null;
        
        if (product == null)
            throw new ArgumentException("Product not found");
        
        workOrder.CalculateTotalMinutes(product, productionLine);
        workOrder.StartDate = DateTime.Now;
        workOrder.Progress = 0;
        workOrder.Status = "InProgress";
        
        if (!availabilityResult.IsAvailable)
            {
                return (null, availabilityResult.MissingMaterials);
            }

        try
        {
            var reserveResult = await _productionService.ReserveMaterials(
                workOrder.ProductId, workOrder.Quantity);
            
            if (!reserveResult)
            {
                return (null, null);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка резервирования материалов");
            return (null, null);
        }

        workOrder.EstimatedEndDate = workOrder.StartDate.Add(
            await _productionService.CalculateProductionTime(
                workOrder.ProductId, workOrder.Quantity, workOrder.ProductionLineId));
        
        workOrder.Status = "InProgress";

        try
        {
            _context.WorkOrders.Add(workOrder);
            await _context.SaveChangesAsync();
            return (workOrder, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании заказа");
            return (null, null);
        }
    }
        public async Task<List<WorkOrder>> GetWorkOrdersAsync(string? status, string? date)
        {
            IQueryable<WorkOrder> query = _context.WorkOrders
                .Include(wo => wo.Product)
                .Include(wo => wo.ProductionLine);

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(wo => wo.Status == status);
            }

            if (date == "today")
            {
                var today = DateTime.Today;
                query = query.Where(wo => wo.StartDate.Date == today);
            }

            return await query.ToListAsync();
        }

        public async Task UpdateWorkOrderProgressAsync(int id, decimal progress)
        {
            var workOrder = await _context.WorkOrders.FindAsync(id);
            if (workOrder == null)
                throw new ArgumentException("Work order not found");

            workOrder.Progress = progress;
            if (progress >= 100)
            {
                workOrder.Status = "Completed";
            }

            await _context.SaveChangesAsync();
        }

        public async Task<WorkOrder?> GetWorkOrderDetailsAsync(int id)
        {
            return await _context.WorkOrders
                .Include(wo => wo.Product)
                .Include(wo => wo.ProductionLine)
                .FirstOrDefaultAsync(wo => wo.Id == id);
        }
    }
}