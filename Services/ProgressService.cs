using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProductionManagementSystem.Data;
using ProductionManagementSystem.Hubs;
using ProductionManagementSystem.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ProductionManagementSystem.Services
{
    public class ProgressService : IProgressService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProgressService> _logger;
        private readonly IHubContext<ProgressHub> _hubContext;

        public ProgressService(
            ApplicationDbContext context,
            ILogger<ProgressService> logger,
            IHubContext<ProgressHub> hubContext)
        {
            _context = context;
            _logger = logger;
            _hubContext = hubContext;
        }

        public async Task CheckAndUpdateWorkOrdersAsync()
{
    try
    {
        var activeOrders = await _context.WorkOrders
            .Where(wo => wo.Status == "InProgress")
            .Include(wo => wo.Product)
            .ToListAsync();

        foreach (var order in activeOrders)
        {
            if (!order.ActualStartDate.HasValue) continue;
            
            var elapsed = DateTime.Now - order.ActualStartDate.Value;
            int minutesPassed = (int)elapsed.TotalMinutes;
            
            int newProgress = (int)((double)minutesPassed / order.TotalMinutesRequired * 100);
            bool isCompleted = minutesPassed >= order.TotalMinutesRequired;
            
            if (order.Progress != newProgress || isCompleted)
            {
                if (isCompleted)
                {
                    order.Status = "Completed";
                    order.Progress = 100;
                    order.ActualEndDate = DateTime.Now;
                    
                    if (order.ProductionLineId.HasValue)
                    {
                        var line = await _context.ProductionLines
                            .FirstOrDefaultAsync(pl => pl.Id == order.ProductionLineId.Value);
                        if (line != null)
                        {
                            line.CurrentWorkOrderId = null;
                        }
                    }
                }
                else
                {
                    order.Progress = newProgress;
                }

                _context.Update(order);
                await _context.SaveChangesAsync();
                _logger.LogInformation("✔ Hangfire вызвал CheckAndUpdateWorkOrdersAsync: {Time}", DateTime.Now);
                _logger.LogInformation("➡ Найдено активных заказов: {Count}", activeOrders.Count);

                await _hubContext.Clients.All.SendAsync(
                    "ReceiveProgressUpdate",
                    order.Id,
                    order.Progress,
                    order.Status,
                    order.MinutesCompleted,
                    order.TotalMinutesRequired);
            }
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error updating work orders");
        throw;
    }
}
    }
}