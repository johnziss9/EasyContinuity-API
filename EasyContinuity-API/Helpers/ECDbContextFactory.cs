using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EasyContinuity_API.Data
{
    public class ECDbContextFactory : IDesignTimeDbContextFactory<ECDbContext>
    {
        public ECDbContext CreateDbContext(string[] args)
        {
            DotNetEnv.Env.Load();
            
            var dbUser = Environment.GetEnvironmentVariable("DB_USER");
            var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");
            
            var connectionString = $"Host=localhost;Port=5437;Database=ecdb_dev;Username={dbUser};Password={dbPassword}";
            
            var optionsBuilder = new DbContextOptionsBuilder<ECDbContext>();
            optionsBuilder.UseNpgsql(connectionString);
            
            return new ECDbContext(optionsBuilder.Options);
        }
    }
}