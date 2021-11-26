using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TG.Queue.Api.Entities;

namespace TG.Queue.Api.Db.EfConfiguration
{
    public class BattleUserConfiguration : IEntityTypeConfiguration<BattleUser>
    {
        public void Configure(EntityTypeBuilder<BattleUser> entity)
        {
            entity.HasKey(bu => new {bu.BattleId, bu.UserId});
            entity.HasOne(bu => bu.Battle)
                .WithMany(b => b!.Users)
                .HasForeignKey(bu => bu.BattleId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}