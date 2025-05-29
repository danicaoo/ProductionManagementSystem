using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProductionManagementSystem.Data;
using ProductionManagementSystem.Models;
using ProductionManagementSystem.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ProductionManagementSystem.Controllers
{
    public class WorkOrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<WorkOrdersController> _logger;

        public WorkOrdersController(ApplicationDbContext context, ILogger<WorkOrdersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string? status, string? date)
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

    var orders = await query.ToListAsync();
    var viewModels = orders.Select(o => new WorkOrderViewModel(o)
    {
        ProductName = o.Product?.Name ?? string.Empty,  
        ProductionLineName = o.ProductionLine?.Name 
    }).ToList();
    
    return View(viewModels);
}

[HttpGet]
public IActionResult Create()
{
    // Обязательно заполняем ViewBag
    ViewBag.Products = new SelectList(_context.Products, "Id", "Name");
    ViewBag.ProductionLines = new SelectList(
        _context.ProductionLines.Where(pl => pl.Status == "Active"), 
        "Id", "Name");
    
    // Устанавливаем значения по умолчанию
    var model = new WorkOrderViewModel
    {
        StartDate = DateTime.Now,
        Status = "Pending",
        Progress = 0
    };
    
    return View(model);
}

     [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create(WorkOrderViewModel viewModel)
{
    try
    {
        // Всегда заполняем списки для повторного отображения
        ViewBag.Products = new SelectList(_context.Products, "Id", "Name");
        ViewBag.ProductionLines = new SelectList(
            _context.ProductionLines.Where(pl => pl.Status == "Active"), 
            "Id", "Name");

        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        var workOrder = viewModel.ToWorkOrder();
        workOrder.EstimatedEndDate = CalculateEstimatedEndDate(viewModel);

        _context.WorkOrders.Add(workOrder);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
    catch (Exception ex)
    {
        ModelState.AddModelError("", $"Ошибка при создании заказа: {ex.Message}");
        return View(viewModel);
    }
}



        [HttpGet]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var workOrder = await _context.WorkOrders
                .Include(wo => wo.Product)
                .Include(wo => wo.ProductionLine)
                .FirstOrDefaultAsync(wo => wo.Id == id);

            if (workOrder == null)
            {
                _logger.LogWarning("Попытка редактирования несуществующего заказа ID: {OrderId}", id);
                return NotFound();
            }

            var viewModel = new WorkOrderViewModel(workOrder)
            {
                ProductName = workOrder.Product?.Name ?? "Неизвестный продукт",
                ProductionLineName = workOrder.ProductionLine?.Name ?? "Не назначена"
            };

            ViewBag.ProductionLines = new SelectList(
                _context.ProductionLines.Where(pl => pl.Status == "Active"), 
                "Id", "Name", workOrder.ProductionLineId);

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, WorkOrderViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                _logger.LogWarning("Несоответствие ID заказа при редактировании. Ожидалось: {ExpectedId}, получено: {ActualId}",
                    id, viewModel.Id);
                return NotFound();
            }

            var existingOrder = await _context.WorkOrders
                .Include(wo => wo.Product)
                .Include(wo => wo.ProductionLine)
                .FirstOrDefaultAsync(wo => wo.Id == id);

            if (existingOrder == null)
            {
                _logger.LogWarning("Заказ с ID {OrderId} не найден при попытке редактирования", id);
                return NotFound();
            }

            if (viewModel.ProductionLineId.HasValue)
            {
                var lineExists = await _context.ProductionLines
                    .AnyAsync(pl => pl.Id == viewModel.ProductionLineId);
                if (!lineExists)
                {
                    ModelState.AddModelError("ProductionLineId", "Указанная производственная линия не существует");
                    _logger.LogWarning("Попытка назначения несуществующей линии {LineId} заказу {OrderId}",
                        viewModel.ProductionLineId, id);
                }
                else
                {
                    existingOrder.ProductionLineId = viewModel.ProductionLineId;
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    existingOrder.Quantity = viewModel.Quantity;
                    existingOrder.Status = viewModel.Status;
                    existingOrder.Progress = viewModel.Progress;
                    existingOrder.Notes = viewModel.Notes;
                    existingOrder.StartDate = viewModel.StartDate;
                    existingOrder.EstimatedEndDate = CalculateEstimatedEndDate(viewModel);

                    _context.Update(existingOrder);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Заказ ID {OrderId} успешно обновлен", id);
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Ошибка при обновлении заказа ID: {OrderId}", id);
                    ModelState.AddModelError("", "Не удалось сохранить изменения. Проверьте корректность данных.");
                }
            }

            viewModel.ProductName = existingOrder.Product?.Name ?? "Неизвестный продукт";
            viewModel.ProductionLineName = existingOrder.ProductionLine?.Name ?? "Не назначена";

            ViewBag.ProductionLines = new SelectList(
                _context.ProductionLines.Where(pl => pl.Status == "Active"),
                "Id", "Name", existingOrder.ProductionLineId);

            return View(viewModel);
        }

        [HttpGet]
public async Task<IActionResult> Details(int id)
{
    try
    {
        var workOrder = await _context.WorkOrders
            .Include(wo => wo.Product)
            .Include(wo => wo.ProductionLine)
            .FirstOrDefaultAsync(wo => wo.Id == id);

        if (workOrder == null)
        {
            _logger.LogWarning("Заказ с ID {OrderId} не найден", id);
            TempData["ErrorMessage"] = "Заказ не найден";
            return RedirectToAction(nameof(Index));
        }
        
        var viewModel = new WorkOrderViewModel(workOrder)
        {
            ProductName = workOrder.Product?.Name ?? "Неизвестный продукт",
            ProductionLineName = workOrder.ProductionLine?.Name ?? "Не назначена"
        };
        
        return View(viewModel);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Ошибка при загрузке деталей заказа ID: {OrderId}", id);
        TempData["ErrorMessage"] = "Произошла ошибка при загрузке данных";
        return RedirectToAction(nameof(Index));
    }
}

        private DateTime CalculateEstimatedEndDate(WorkOrderViewModel viewModel)
        {
            return viewModel.StartDate.AddHours(viewModel.Quantity * 0.5);
        }

        private bool WorkOrderExists(int id)
        {
            return _context.WorkOrders.Any(e => e.Id == id);
        }
    }
}