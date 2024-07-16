using FootballManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FootballManager.Domain.Settings
{
    public class TeamSettings : IEntityTypeConfiguration<Team>
    {
        public void Configure(EntityTypeBuilder<Team> builder)
        {
            builder.ToTable("Teams").HasKey(team => team.Id);

            builder.Property(team => team.Name).IsRequired();
            builder.Property(team => team.SeasonPlacement);

            builder.HasOne(team => team.Manager).WithMany(user => user.Teams).HasForeignKey(team => team.ManagerId).HasPrincipalKey(user => user.Id).OnDelete(DeleteBehavior.NoAction);
            builder.HasOne(team => team.Season).WithMany(season => season.Teams).HasForeignKey(team => team.SeasonId).HasPrincipalKey(season => season.Id).OnDelete(DeleteBehavior.NoAction);

            builder.Property(team => team.DateCreated).IsRequired().HasDefaultValueSql("GETUTCDATE()");
        }
    }
}
