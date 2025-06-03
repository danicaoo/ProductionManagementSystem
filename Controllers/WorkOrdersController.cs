using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProductionManagementSystem.Data;
using ProductionManagementSystem.Models;
using ProductionManagementSystem.Services;
using ProductionManagementSystem.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;

namespace ProductionManagementSystem.Controllers
{
    public class WorkOrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<WorkOrdersController> _logger;
        private readonly IWorkOrderService _workOrderService;
        private readonly IProductionService _productionService;
        private readonly IProgressService _progressService;

        public WorkOrdersController(
            ApplicationDbContext context,
            ILogger<WorkOrdersController> logger,
            IWorkOrderService workOrderService,
            IProductionService productionService,
            IProgressService progressService)
        {
            _context = context;
            _logger = logger;
            _workOrderService = workOrderService;
            _productionService = productionService;
            _progressService = progressService;
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
            var viewModels = orders.Select(o =>
            {
                var vm = new WorkOrderViewModel(o);
                vm.ProductName = o.Product?.Name ?? string.Empty;
                vm.ProductionLineName = o.ProductionLine?.Name ?? "Не назначена";
                return vm;
            }).ToList();

            return View(viewModels);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Products = new SelectList(_context.Products, "Id", "Name");
            ViewBag.ProductionLines = new SelectList(
                _context.ProductionLines.Where(pl => pl.Status == "Active"),
                "Id", "Name");

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
                ViewBag.Products = new SelectList(_context.Products, "Id", "Name");
                ViewBag.ProductionLines = new SelectList(
                    _context.ProductionLines.Where(pl => pl.Status == "Active"),
                    "Id", "Name");

                if (!ModelState.IsValid)
                {
                    return View(viewModel);
                }

                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    var workOrder = viewModel.ToWorkOrder();
                    var product = await _context.Products.FindAsync(workOrder.ProductId);
                    var productionLine = workOrder.ProductionLineId.HasValue
                        ? await _context.ProductionLines.FindAsync(workOrder.ProductionLineId.Value)
                        : null;

                    if (product == null)
                    {
                        TempData["ErrorMessage"] = "Продукт не найден";
                        return View(viewModel);
                    }

                    var availability = await _productionService.CheckMaterialsAvailability(
                        workOrder.ProductId, workOrder.Quantity);

                    if (!availability.IsAvailable)
                    {
                        var missingList = availability.MissingMaterials.Select(kvp =>
                            $"{_context.Materials.Find(kvp.Key)?.Name}: не хватает {kvp.Value}");
                        TempData["ErrorMessage"] = $"Недостаточно материалов: {string.Join(", ", missingList)}";
                        return View(viewModel);
                    }

                    var reserveResult = await _productionService.ReserveMaterials(
                        workOrder.ProductId, workOrder.Quantity);

                    if (!reserveResult)
                    {
                        TempData["ErrorMessage"] = "Ошибка при резервировании материалов";
                        return View(viewModel);
                    }

                    workOrder.CalculateTotalMinutes(product, productionLine);
                    workOrder.ActualStartDate = DateTime.Now;
                    workOrder.Status = "InProgress";
                    workOrder.Progress = 0;
                    workOrder.EstimatedEndDate = workOrder.StartDate.Add(
                        await _productionService.CalculateProductionTime(
                            workOrder.ProductId, workOrder.Quantity, workOrder.ProductionLineId));
                    await _context.SaveChangesAsync();



                    _context.WorkOrders.Add(workOrder);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Ошибка при создании заказа");
                    TempData["ErrorMessage"] = $"Ошибка: {ex.Message}";
                    return View(viewModel);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании заказа");
                TempData["ErrorMessage"] = $"Ошибка: {ex.Message}";
                return View(viewModel);
            }
        }

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
            var workOrder = await _context.WorkOrders
                .Include(wo => wo.Product)
                .Include(wo => wo.ProductionLine)
                .FirstOrDefaultAsync(wo => wo.Id == id);

            if (workOrder == null) return NotFound();

            if (workOrder.Product != null && workOrder.Status == "InProgress")
            {
                workOrder.CalculateTotalMinutes(workOrder.Product, workOrder.ProductionLine);
                await _context.SaveChangesAsync();
            }

            var viewModel = new WorkOrderViewModel(workOrder);
            return View(viewModel);
        }
    }
}
