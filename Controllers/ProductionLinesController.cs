using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductionManagementSystem.Data;
using ProductionManagementSystem.Models;
using ProductionManagementSystem.ViewModels;

namespace ProductionManagementSystem.Controllers
{
    public class ProductionLinesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductionLinesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(bool? available)
        {
            IQueryable<ProductionLine> query = _context.ProductionLines;

            if (available == true)
            {
                query = query.Where(pl => pl.Status == "Active" && pl.CurrentWorkOrderId == null);
            }

            var lines = await query.ToListAsync();
            var viewModels = lines.Select(l => new ProductionLineViewModel(l)).ToList();

            return View(viewModels);
        }

        public async Task<IActionResult> Details(int id)
        {
            var line = await _context.ProductionLines
                .Include(pl => pl.WorkOrders)
                .FirstOrDefaultAsync(pl => pl.Id == id);

            if (line == null) return NotFound();

            return View(new ProductionLineViewModel(line));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var line = await _context.ProductionLines.FindAsync(id);
            if (line == null) return NotFound();

            return View(new ProductionLineViewModel(line));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductionLineViewModel viewModel)
        {
            if (id != viewModel.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var line = viewModel.ToProductionLine();
                _context.Update(line);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(viewModel);
        }
        [HttpGet]
        [HttpGet]
        public IActionResult Create()
        {
            return View(new ProductionLineViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductionLineViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var productionLine = new ProductionLine
                {
                    Name = viewModel.Name,
                    Status = viewModel.Status,
                    EfficiencyFactor = viewModel.EfficiencyFactor
                };

                _context.ProductionLines.Add(productionLine);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(viewModel);
        }
    }
}