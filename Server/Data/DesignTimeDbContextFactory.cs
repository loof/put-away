using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PutAway.Server.Data
{
    public abstract class DesignTimeDbContextFactoryBase<TContext> :
        IDesignTimeDbContextFactory<TContext> where TContext : DbContext
    {
        protected string ConnectionStringName { get; }
        protected string MigrationsAssemblyName { get; }

        public DesignTimeDbContextFactoryBase(string connectionStringName, string migrationsAssemblyName)
        {
            ConnectionStringName = connectionStringName;
            MigrationsAssemblyName = migrationsAssemblyName;
        }

        public TContext CreateDbContext(string[] args)
        {
            return Create(
                Directory.GetCurrentDirectory(),
                Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                ConnectionStringName, MigrationsAssemblyName);
        }
        protected abstract TContext CreateNewInstance(
            DbContextOptions<TContext> options);

        public TContext CreateWithConnectionStringName(string connectionStringName, string migrationsAssemblyName)
        {
            var environmentName =
                Environment.GetEnvironmentVariable(
                    "ASPNETCORE_ENVIRONMENT");

            var basePath = AppContext.BaseDirectory;

            return Create(basePath, environmentName, connectionStringName, migrationsAssemblyName);
        }

        private TContext Create(string basePath, string environmentName, string connectionStringName, string migrationsAssemblyName)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{environmentName}.json", true)
                .AddEnvironmentVariables();

            var config = builder.Build();

            var connstr = config.GetConnectionString(connectionStringName);
            Console.WriteLine($"Environment: {environmentName ?? "PRODUCTION"}");

            if (string.IsNullOrWhiteSpace(connstr))
            {
                throw new InvalidOperationException(
                    $"Could not find a connection string named '{connectionStringName}'.");
            }
            else
            {
                return CreateWithConnectionString(connectionStringName, connstr, migrationsAssemblyName);
            }
        }

        private TContext CreateWithConnectionString(string connectionStringName, string connectionString, string migrationsAssembly)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentException(
             $"{nameof(connectionString)} is null or empty.",
             nameof(connectionString));

            var optionsBuilder =
                 new DbContextOptionsBuilder<TContext>();

            Console.WriteLine(
                "DesignTimeDbContextFactory.Create(string): Connection string: {0}",
                connectionStringName);

            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production")
            {
                optionsBuilder.UseNpgsql(connectionString, db => db.MigrationsAssembly(migrationsAssembly));
                Console.WriteLine("Using Postgres.");
            }
            else
            {
                optionsBuilder.UseSqlite(connectionString, db => db.MigrationsAssembly(migrationsAssembly));
                Console.WriteLine("Using Sqlite.");
            }
            
            DbContextOptions<TContext> options = optionsBuilder.Options;

            return CreateNewInstance(options);
        }
    }
}