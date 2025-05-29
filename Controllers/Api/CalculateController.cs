using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductionManagementSystem.Data;
using System.Threading.Tasks;

namespace ProductionManagementSystem.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class CalculateController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CalculateController(ApplicationDbContext context)
        {
            _context = context;
        }

        public class ProductionTimeRequest
        {
            public int ProductId { get; set; }
            public int Quantity { get; set; }
            public int? ProductionLineId { get; set; }
        }

        public class ProductionTimeResult
        {
            public double TotalMinutes { get; set; }
            public string FormattedTime { get; set; } = string.Empty;
        }

        [HttpPost("production")]
        public async Task<ActionResult<ProductionTimeResult>> CalculateProductionTime(
            [FromBody] ProductionTimeRequest request)
        {
            var product = await _context.Products.FindAsync(request.ProductId);
            if (product == null)
            {
                return BadRequest("Продукт не найден");
            }

            var productionLine = await _context.ProductionLines.FindAsync(request.ProductionLineId);
            var efficiency = productionLine?.EfficiencyFactor ?? 1.0f;

            var totalMinutes = (request.Quantity * product.ProductionTimePerUnit) / efficiency;
            var timeSpan = TimeSpan.FromMinutes(totalMinutes);

            return new ProductionTimeResult
            {
                TotalMinutes = totalMinutes,
                FormattedTime = $"{timeSpan.Days} дней {timeSpan.Hours} часов {timeSpan.Minutes} минут"
            };
        }
    }
}