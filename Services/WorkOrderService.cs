using Microsoft.EntityFrameworkCore;
using ProductionManagementSystem.Data;
using ProductionManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProductionManagementSystem.Services
{
    public class WorkOrderService : IWorkOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly IProductionService _productionService;

        public WorkOrderService(ApplicationDbContext context, IProductionService productionService)
        {
            _context = context;
            _productionService = productionService;
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

        public async Task<WorkOrder> CreateWorkOrderAsync(WorkOrder workOrder)
        {
            var materialsAvailable = await _productionService.CheckMaterialsAvailability(
                workOrder.ProductId, workOrder.Quantity);
            
            if (!materialsAvailable)
            {
                throw new InvalidOperationException("Not enough materials available");
            }

            var productionTime = await _productionService.CalculateProductionTime(
                workOrder.ProductId, workOrder.Quantity, workOrder.ProductionLineId);
            
            workOrder.EstimatedEndDate = workOrder.StartDate.Add(productionTime);

            var materialsReserved = await _productionService.ReserveMaterials(
                workOrder.ProductId, workOrder.Quantity);
            
            if (!materialsReserved)
            {
                throw new InvalidOperationException("Failed to reserve materials");
            }

            _context.WorkOrders.Add(workOrder);
            await _context.SaveChangesAsync();

            return workOrder;
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