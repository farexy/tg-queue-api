using Microsoft.EntityFrameworkCore;
using TG.Core.Db.Postgres;

namespace TG.Queue.Api.Db
{
    public class ApplicationDbContext : TgDbContext
    {
        public DbSet<BattleServer> BattleServers { get; set; } = default!;

        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }
    }
}