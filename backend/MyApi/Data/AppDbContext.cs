using Microsoft.EntityFrameworkCore;
using MyApi.Entities;

namespace MyApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) {}

    public DbSet<InventoryValuation> InventoryValuations => Set<InventoryValuation>();
    public DbSet<InventoryValuationItem> InventoryValuationItems => Set<InventoryValuationItem>();
}
