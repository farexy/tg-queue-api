using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using TG.Queue.Api.Db;

namespace TG.Manager.Service.Db
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseNpgsql();

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}