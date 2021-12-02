using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TG.Queue.Api.Entities;

namespace TG.Queue.Api.Db.EfConfiguration
{
    public class BattleConfiguration : IEntityTypeConfiguration<Battle>
    {
        public void Configure(EntityTypeBuilder<Battle> entity)
        {
            entity.HasKey(b => b.Id);
            entity.HasIndex(b => b.BattleType);
        }
    }
}