using FootballManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FootballManager.Domain.Settings
{
    public class SeasonPassSettings : IEntityTypeConfiguration<SeasonPass>
    {
        public void Configure(EntityTypeBuilder<SeasonPass> builder)
        {
            builder.ToTable("SeasonPasses").HasKey(seasonpass => seasonpass.Id);

            builder.Property(seasonpass => seasonpass.Password).IsRequired();
            builder.Property(seasonpass => seasonpass.DateUsed);

            builder.HasOne(player => player.Manager).WithMany(manager => manager.SeasonPasses).HasForeignKey(player => player.ManagerId).HasPrincipalKey(manager => manager.Id).OnDelete(DeleteBehavior.NoAction);
            builder.HasOne(player => player.Team).WithMany(team => team.SeasonPassess).HasForeignKey(player => player.TeamId).HasPrincipalKey(team => team.Id).OnDelete(DeleteBehavior.NoAction);
            builder.HasOne(player => player.Season).WithMany(season => season.SeasonPasses).HasForeignKey(player => player.SeasonId).HasPrincipalKey(season => season.Id).OnDelete(DeleteBehavior.NoAction);

            builder.Property(seasonpass => seasonpass.DateCreated).IsRequired().HasDefaultValueSql("GETUTCDATE()");
        }
    }
}
