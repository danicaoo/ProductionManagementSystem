using Microsoft.EntityFrameworkCore;
using ProductionManagementSystem.Models;

namespace ProductionManagementSystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<ProductionLine> ProductionLines { get; set; }
        public DbSet<Material> Materials { get; set; }
        public DbSet<ProductMaterial> ProductMaterials { get; set; }
        public DbSet<WorkOrder> WorkOrders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Конфигурация для Material
            modelBuilder.Entity<Material>(entity =>
            {
                entity.Property(m => m.Quantity).HasPrecision(18, 2);
                entity.Property(m => m.MinimalStock).HasPrecision(18, 2);
            });

            // Конфигурация для ProductMaterial
            modelBuilder.Entity<ProductMaterial>(entity =>
            {
                entity.HasKey(pm => new { pm.ProductId, pm.MaterialId });
                entity.Property(pm => pm.QuantityNeeded).HasPrecision(18, 2);

                entity.HasOne(pm => pm.Product)
                      .WithMany(p => p.ProductMaterials)
                      .HasForeignKey(pm => pm.ProductId);

                entity.HasOne(pm => pm.Material)
                      .WithMany(m => m.ProductMaterials)
                      .HasForeignKey(pm => pm.MaterialId);
            });

            // Конфигурация для WorkOrder
            modelBuilder.Entity<WorkOrder>(entity =>
            {
                entity.Property(wo => wo.Progress).HasPrecision(5, 2);

                entity.HasOne(wo => wo.Product)
                      .WithMany()
                      .HasForeignKey(wo => wo.ProductId);

                entity.HasOne(wo => wo.ProductionLine)
                      .WithMany(pl => pl.WorkOrders)
                      .HasForeignKey(wo => wo.ProductionLineId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            base.OnModelCreating(modelBuilder);
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseSqlite("Data Source=Production.db")
                    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            }

    }
    
    
}