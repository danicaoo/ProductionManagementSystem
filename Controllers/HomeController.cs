using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductionManagementSystem.Data;
using ProductionManagementSystem.ViewModels;

namespace ProductionManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var model = new HomeDashboardViewModel
            {
                ActiveOrders = await _context.WorkOrders.CountAsync(w => w.Status == "InProgress"),
                PendingOrders = await _context.WorkOrders.CountAsync(w => w.Status == "Pending"),
                LowStockMaterials = await _context.Materials.CountAsync(m => m.Quantity < m.MinimalStock),
                ActiveLines = await _context.ProductionLines.CountAsync(pl => pl.Status == "Active"),
                RecentWorkOrders = await _context.WorkOrders
                    .Include(w => w.Product)
                    .Include(w => w.ProductionLine)
                    .OrderByDescending(w => w.StartDate)
                    .Take(5)
                    .Select(w => new WorkOrderViewModel
                    {
                        Id = w.Id,
                        ProductName = w.Product.Name ?? string.Empty,
                        ProductionLineName = w.ProductionLine.Name ?? string.Empty,
                        Quantity = w.Quantity,
                        StartDate = w.StartDate,
                        EstimatedEndDate = w.EstimatedEndDate,
                        Status = w.Status ?? "Pending",
                        Progress = w.Progress
                    })
                    .ToListAsync()
            };

            return View(model);
        }
    }
}