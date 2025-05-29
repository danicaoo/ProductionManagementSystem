using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductionManagementSystem.Data;
using ProductionManagementSystem.Models;

namespace ProductionManagementSystem.Controllers.Api
{
    [Route("api/workorders")]
    [ApiController]
    public class WorkOrdersApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public WorkOrdersApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<WorkOrder>>> GetWorkOrders(string? status, string? date)
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

        [HttpPost]
        public async Task<ActionResult<WorkOrder>> CreateWorkOrder([FromBody] WorkOrder workOrder)
        {
            // Проверка материалов и расчет времени
            _context.WorkOrders.Add(workOrder);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetWorkOrders), new { id = workOrder.Id }, workOrder);
        }

        [HttpPut("{id}/progress")]
        public async Task<IActionResult> UpdateProgress(int id, [FromBody] decimal percent)
        {
            var workOrder = await _context.WorkOrders.FindAsync(id);
            if (workOrder == null) return NotFound();
            
            workOrder.Progress = percent;
            if (percent >= 100) workOrder.Status = "Completed";
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}