using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductionManagementSystem.Data;
using ProductionManagementSystem.ViewModels;
using System.Linq;
using System.Threading.Tasks;

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
            var recentOrders = await _context.WorkOrders
                .Include(w => w.Product)
                .Include(w => w.ProductionLine)
                .OrderByDescending(w => w.StartDate)
                .Take(5)
                .ToListAsync();

            var model = new HomeDashboardViewModel
            {
                ActiveOrders = await _context.WorkOrders.CountAsync(w => w.Status == "InProgress"),
                PendingOrders = await _context.WorkOrders.CountAsync(w => w.Status == "Pending"),
                LowStockMaterials = await _context.Materials.CountAsync(m => m.Quantity < m.MinimalStock),
                ActiveLines = await _context.ProductionLines.CountAsync(pl => pl.Status == "Active"),
                TotalProducts = await _context.Products.CountAsync(),
                RecentWorkOrders = recentOrders.Select(w => new WorkOrderViewModel
                {
                    Id = w.Id,
                    ProductName = w.Product?.Name ?? "Неизвестно",
                    ProductionLineName = w.ProductionLine?.Name ?? "Не назначена",
                    Quantity = w.Quantity,
                    StartDate = w.StartDate,
                    EstimatedEndDate = w.EstimatedEndDate,
                    Status = w.Status ?? "Pending",
                    Progress = w.Progress
                }).ToList()
            };

            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> GetDashboardCounters()
        {
            var counters = new
            {
                activeOrders = await _context.WorkOrders.CountAsync(w => w.Status == "InProgress"),
                pendingOrders = await _context.WorkOrders.CountAsync(w => w.Status == "Pending"),
                lowStockMaterials = await _context.Materials.CountAsync(m => m.Quantity < m.MinimalStock),
                activeLines = await _context.ProductionLines.CountAsync(pl => pl.Status == "Active"),
                totalProducts = await _context.Products.CountAsync()
            };
            
            return Json(counters);
        }
    }
}