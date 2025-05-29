using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductionManagementSystem.Data;
using ProductionManagementSystem.Models;
using ProductionManagementSystem.ViewModels;

namespace ProductionManagementSystem.Controllers
{
    public class MaterialsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MaterialsController(ApplicationDbContext context)
        {  
            _context = context;
        }

        public async Task<IActionResult> Index(bool? lowStock)
        {
            IQueryable<Material> query = _context.Materials;

            if (lowStock == true)
            {
                query = query.Where(m => m.Quantity < m.MinimalStock);
            }

            var materials = await query.ToListAsync();
            var viewModels = materials.Select(m => new MaterialViewModel(m)).ToList();

            return View(viewModels);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MaterialViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var material = viewModel.ToMaterial();
                _context.Materials.Add(material);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(viewModel);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var material = await _context.Materials.FindAsync(id);
            if (material == null) return NotFound();

            return View(new MaterialViewModel(material));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MaterialViewModel viewModel)
        {
            if (id != viewModel.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var material = viewModel.ToMaterial();
                _context.Update(material);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(viewModel);
        }
        public async Task<IActionResult> Details(int? id)
{
    Console.WriteLine($"Details action called with id: {id}");
    
    if (id == null)
    {
        Console.WriteLine("ID is null");
        return NotFound();
    }

    var material = await _context.Materials
        .FirstOrDefaultAsync(m => m.Id == id);
        
    if (material == null)
    {
        Console.WriteLine($"Material with id {id} not found");
        return NotFound();
    }

    Console.WriteLine($"Found material: {material.Name}");
    var viewModel = new MaterialViewModel(material);
    return View(viewModel);
}
    }
}