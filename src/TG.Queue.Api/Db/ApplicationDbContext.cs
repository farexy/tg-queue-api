using Microsoft.EntityFrameworkCore;
using TG.Core.Db.Postgres;
using TG.Queue.Api.Entities;

namespace TG.Queue.Api.Db
{
    public class ApplicationDbContext : TgDbContext
    {
        public DbSet<Battle> Battles { get; set; } = default!;
        public DbSet<BattleUser> BattleUsers { get; set; } = default!;

        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }
    }
}