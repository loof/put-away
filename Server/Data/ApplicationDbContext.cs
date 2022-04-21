using System.Reflection;
using Microsoft.EntityFrameworkCore;
using PutAway.Shared.Entities;

namespace PutAway.Server.Data;

public class ApplicationDbContext : DbContext
{
    public DbSet<Item> Items { get; set; }
    public DbSet<Image> Images { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}

public class ApplicationContextDesignFactory : DesignTimeDbContextFactoryBase<ApplicationDbContext>
{
    public ApplicationContextDesignFactory() : base("DefaultConnection", typeof(Program).GetTypeInfo().Assembly.GetName().Name)
    { }
    protected override ApplicationDbContext CreateNewInstance(DbContextOptions<ApplicationDbContext> options)
    {
        return new ApplicationDbContext(options);
    }
}