using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProductionManagementSystem.Data;
using ProductionManagementSystem.Models;
using ProductionManagementSystem.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductionManagementSystem.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(string? category)
        {
            IQueryable<Product> query = _context.Products;

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(p => p.Category == category);
            }

            var products = await query.ToListAsync();
            var viewModels = products.Select(p => new ProductViewModel(p)).ToList();

            return View(viewModels);
        }

        [HttpGet]
        [HttpGet]
        public IActionResult Create()
        {
            // Загружаем список всех материалов
            var materials = _context.Materials.ToList();

            // Создаем модель и передаем список материалов
            var viewModel = new ProductViewModel
            {
                ProductMaterials = new List<ProductViewModel.ProductMaterialViewModel>()
            };

            if (materials.Any())
            {
                viewModel.ProductMaterials.Add(new ProductViewModel.ProductMaterialViewModel());
            }

            ViewBag.MaterialsList = new SelectList(materials, "Id", "Name");
            ViewBag.AllMaterials = materials; 
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var product = viewModel.ToProduct();
                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                if (viewModel.ProductMaterials != null)
                {
                    foreach (var materialVm in viewModel.ProductMaterials)
                    {
                        if (materialVm.QuantityNeeded > 0) 
                        {
                            var material = await _context.Materials.FindAsync(materialVm.MaterialId);
                            if (material != null)
                            {
                                var productMaterial = new ProductMaterial
                                {
                                    ProductId = product.Id,
                                    MaterialId = materialVm.MaterialId,
                                    QuantityNeeded = materialVm.QuantityNeeded,
                                    UnitOfMeasure = material.UnitOfMeasure
                                };
                                _context.ProductMaterials.Add(productMaterial);
                            }
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }

            ViewBag.Materials = new SelectList(_context.Materials.ToList(), "Id", "Name");
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products
                .Include(p => p.ProductMaterials)
                .ThenInclude(pm => pm.Material)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            var viewModel = new ProductViewModel(product);
            ViewBag.Materials = new SelectList(_context.Materials.ToList(), "Id", "Name");
            ViewBag.AllMaterials = _context.Materials.ToList();

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var product = viewModel.ToProduct();
                    _context.Update(product);

                    var existingMaterials = await _context.ProductMaterials
                        .Where(pm => pm.ProductId == id)
                        .ToListAsync();

                    _context.ProductMaterials.RemoveRange(existingMaterials);

                    if (viewModel.ProductMaterials != null)
                    {
                        foreach (var materialVm in viewModel.ProductMaterials)
                        {
                            var material = await _context.Materials.FindAsync(materialVm.MaterialId);
                            if (material != null)
                            {
                                var productMaterial = new ProductMaterial
                                {
                                    ProductId = product.Id,
                                    MaterialId = materialVm.MaterialId,
                                    QuantityNeeded = materialVm.QuantityNeeded,
                                    UnitOfMeasure = material.UnitOfMeasure
                                };
                                _context.ProductMaterials.Add(productMaterial);
                            }
                        }
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Materials = new SelectList(_context.Materials.ToList(), "Id", "Name");
            ViewBag.AllMaterials = _context.Materials.ToList();
            return View(viewModel);
        }

        public async Task<IActionResult> Materials(int id)
        {
            Console.WriteLine($"Запрошены материалы для продукта ID: {id}");

            var product = await _context.Products
                .Include(p => p.ProductMaterials)
                .ThenInclude(pm => pm.Material)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                Console.WriteLine($"Продукт с ID {id} не найден");
                return NotFound();
            }

            Console.WriteLine($"Найдено материалов: {product.ProductMaterials?.Count ?? 0}");

            if (product.ProductMaterials != null)
            {
                foreach (var pm in product.ProductMaterials)
                {
                    Console.WriteLine($"- Материал ID: {pm.MaterialId}, Название: {pm.Material?.Name}, Количество: {pm.QuantityNeeded}");
                }
            }

            var viewModel = new ProductViewModel(product);
            return View(viewModel);
        }

        [HttpGet]
        public IActionResult AddMaterialToProduct(int id)
        {
            ViewBag.ProductId = id;
            ViewBag.Materials = new SelectList(_context.Materials, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMaterialToProduct(ProductMaterialViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var material = await _context.Materials.FindAsync(viewModel.MaterialId);
                if (material != null)
                {
                    var existingLink = await _context.ProductMaterials
                        .FirstOrDefaultAsync(pm =>
                            pm.ProductId == viewModel.ProductId &&
                            pm.MaterialId == viewModel.MaterialId);

                    if (existingLink != null)
                    {
                        existingLink.QuantityNeeded += viewModel.QuantityNeeded;
                    }
                    else
                    {
                        var productMaterial = new ProductMaterial
                        {
                            ProductId = viewModel.ProductId,
                            MaterialId = viewModel.MaterialId,
                            QuantityNeeded = viewModel.QuantityNeeded
                        };
                        _context.ProductMaterials.Add(productMaterial);
                    }

                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Materials), new { id = viewModel.ProductId });
                }
            }

            ViewBag.ProductId = viewModel.ProductId;
            ViewBag.Materials = new SelectList(_context.Materials, "Id", "Name");
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveMaterialFromProduct(int productId, int materialId)
        {
            var productMaterial = await _context.ProductMaterials
                .FirstOrDefaultAsync(pm => pm.ProductId == productId && pm.MaterialId == materialId);

            if (productMaterial != null)
            {
                _context.ProductMaterials.Remove(productMaterial);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Materials), new { id = productId });
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
        
    }
        
    }