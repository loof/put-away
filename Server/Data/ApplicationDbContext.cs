using System.Reflection;
using Microsoft.EntityFrameworkCore;
using PutAway.Shared.Entities;

namespace PutAway.Server.Data;

public class ApplicationDbContext : DbContext
{
    private const int NumberOfItemsToGenerate = 5;
    public DbSet<Item> Items { get; set; }
    public DbSet<Image> Images { get; set; }
    public DbSet<PutAway.Shared.Entities.User> Users { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasIndex(u => u.EmailAddress)
            .IsUnique();

        
        int primaryKeyTag = 1;
        int primaryKeyItem = 1;

        for (int i = 1; i <= NumberOfItemsToGenerate; i++)
        {
            modelBuilder.Entity<Item>().HasData(new Item {
                Id = primaryKeyItem,
                Name = $"Item Name {primaryKeyItem}",
                Description = "Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor."
            });
            primaryKeyItem++;
        }
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