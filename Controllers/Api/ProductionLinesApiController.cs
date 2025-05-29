using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductionManagementSystem.Data;
using ProductionManagementSystem.Models;

namespace ProductionManagementSystem.Controllers.Api
{
    [Route("api/productionlines")]
    [ApiController]
    public class ProductionLinesApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProductionLinesApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductionLine>>> GetProductionLines(bool? available)
        {
            IQueryable<ProductionLine> query = _context.ProductionLines;
            
            if (available == true)
            {
                query = query.Where(pl => pl.Status == "Active" && pl.CurrentWorkOrderId == null);
            }

            return await query.ToListAsync();
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status)
        {
            var productionLine = await _context.ProductionLines.FindAsync(id);
            if (productionLine == null) return NotFound();
            
            productionLine.Status = status;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("{id}/schedule")]
        public async Task<ActionResult<IEnumerable<WorkOrder>>> GetSchedule(int id)
        {
            var productionLine = await _context.ProductionLines
                .Include(pl => pl.WorkOrders)
                .FirstOrDefaultAsync(pl => pl.Id == id);

            if (productionLine == null) return NotFound();
            
            return productionLine.WorkOrders.ToList();
        }
    }
}