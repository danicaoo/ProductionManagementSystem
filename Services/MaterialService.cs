using Microsoft.EntityFrameworkCore;
using ProductionManagementSystem.Data;
using ProductionManagementSystem.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProductionManagementSystem.Services
{
    public class MaterialService : IMaterialService
    {
        private readonly ApplicationDbContext _context;

        public MaterialService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Material>> GetMaterialsAsync(bool? lowStock)
        {
            IQueryable<Material> query = _context.Materials;
            
            if (lowStock == true)
            {
                query = query.Where(m => m.Quantity < m.MinimalStock);
            }

            return await query.ToListAsync();
        }

        public async Task<Material?> GetMaterialAsync(int id)
        {
            return await _context.Materials.FindAsync(id);
        }

        public async Task<Material> CreateMaterialAsync(Material material)
        {
            _context.Materials.Add(material);
            await _context.SaveChangesAsync();
            return material;
        }

        public async Task UpdateMaterialStockAsync(int id, decimal amount)
        {
            var material = await _context.Materials.FindAsync(id);
            if (material == null)
                throw new ArgumentException("Material not found");

            material.Quantity += amount;
            await _context.SaveChangesAsync();
        }
    }
}