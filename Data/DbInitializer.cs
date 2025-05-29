using ProductionManagementSystem.Models;

namespace ProductionManagementSystem.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            context.Database.EnsureCreated();

            if (context.Products.Any() || context.Materials.Any() || 
                context.ProductionLines.Any() || context.WorkOrders.Any())
            {
                return; 
            }

            var materials = new Material[]
            {
                new Material { Name = "Сталь", Quantity = 1000, UnitOfMeasure = "кг", MinimalStock = 500 },
                new Material { Name = "Пластик", Quantity = 500, UnitOfMeasure = "кг", MinimalStock = 200 },
                new Material { Name = "Электронные компоненты", Quantity = 200, UnitOfMeasure = "шт", MinimalStock = 100 },
                new Material { Name = "Краска", Quantity = 300, UnitOfMeasure = "литр", MinimalStock = 100 }
            };
            context.Materials.AddRange(materials);
            context.SaveChanges();

            var products = new Product[]
            {
                new Product { 
                    Name = "Металлический стул", 
                    Description = "Прочный металлический стул", 
                    Category = "Мебель", 
                    MinimalStock = 50, 
                    ProductionTimePerUnit = 30 
                },
                new Product { 
                    Name = "Пластиковый стол", 
                    Description = "Легкий пластиковый стол", 
                    Category = "Мебель", 
                    MinimalStock = 30, 
                    ProductionTimePerUnit = 45 
                },
                new Product { 
                    Name = "Электронный контроллер", 
                    Description = "Управляющий контроллер для оборудования", 
                    Category = "Электроника", 
                    MinimalStock = 20, 
                    ProductionTimePerUnit = 60 
                }
            };
            context.Products.AddRange(products);
            context.SaveChanges();

            var productMaterials = new ProductMaterial[]
            {
                new ProductMaterial { ProductId = 1, MaterialId = 1, QuantityNeeded = 5 },
                new ProductMaterial { ProductId = 1, MaterialId = 4, QuantityNeeded = 0.5m },
                new ProductMaterial { ProductId = 2, MaterialId = 2, QuantityNeeded = 8 },
                new ProductMaterial { ProductId = 3, MaterialId = 3, QuantityNeeded = 15 }
            };
            context.ProductMaterials.AddRange(productMaterials);
            context.SaveChanges();

            var productionLines = new ProductionLine[]
            {
                new ProductionLine { Name = "Линия сборки мебели", Status = "Active", EfficiencyFactor = 1.0f },
                new ProductionLine { Name = "Линия сборки электроники", Status = "Active", EfficiencyFactor = 1.2f },
                new ProductionLine { Name = "Линия покраски", Status = "Stopped", EfficiencyFactor = 0.8f }
            };
            context.ProductionLines.AddRange(productionLines);
            context.SaveChanges();

            var workOrders = new WorkOrder[]
            {
                new WorkOrder { 
                    ProductId = 1, 
                    ProductionLineId = 1, 
                    Quantity = 10, 
                    StartDate = DateTime.Now, 
                    EstimatedEndDate = DateTime.Now.AddHours(5), 
                    Status = "InProgress" 
                },
                new WorkOrder { 
                    ProductId = 2, 
                    ProductionLineId = 1, 
                    Quantity = 5, 
                    StartDate = DateTime.Now.AddDays(1), 
                    EstimatedEndDate = DateTime.Now.AddDays(1).AddHours(4), 
                    Status = "Pending" 
                },
                new WorkOrder { 
                    ProductId = 3, 
                    ProductionLineId = 2, 
                    Quantity = 8, 
                    StartDate = DateTime.Now.AddDays(-1), 
                    EstimatedEndDate = DateTime.Now.AddHours(2), 
                    Status = "Completed" 
                }
            };
            context.WorkOrders.AddRange(workOrders);
            context.SaveChanges();
        }
    }
}