using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TG.Queue.Api.Db.EfConfiguration
{
    public class BattleServerConfiguration : IEntityTypeConfiguration<BattleServer>
    {
        public void Configure(EntityTypeBuilder<BattleServer> entity)
        {
            entity.HasKey(bs => bs.BattleId);
        }
    }
}