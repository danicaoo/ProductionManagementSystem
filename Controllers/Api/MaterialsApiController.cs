using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductionManagementSystem.Data;
using ProductionManagementSystem.Models;

namespace ProductionManagementSystem.Controllers.Api
{
    [Route("api/materials")]
    [ApiController]
    public class MaterialsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MaterialsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Material>>> GetMaterials(bool? lowStock)
        {
            IQueryable<Material> query = _context.Materials;
            
            if (lowStock == true)
            {
                query = query.Where(m => m.Quantity < m.MinimalStock);
            }

            return await query.ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<Material>> PostMaterial([FromBody] Material material)
        {
            _context.Materials.Add(material);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetMaterials), new { id = material.Id }, material);
        }

        [HttpPut("{id}/stock")]
        public async Task<IActionResult> UpdateStock(int id, [FromBody] decimal amount)
        {
            var material = await _context.Materials.FindAsync(id);
            if (material == null) return NotFound();
            
            material.Quantity += amount;
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}