using FootballManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FootballManager.Domain.Settings
{
    public class PlayerSettings : IEntityTypeConfiguration<Player>
    {
        public void Configure(EntityTypeBuilder<Player> builder)
        {
            builder.ToTable("Players").HasKey(player => player.Id);

            builder.Property(player => player.Name).IsRequired();
            builder.Property(player => player.Position).IsRequired();
            builder.Property(player => player.Number).IsRequired();

            builder.HasOne(player => player.Team).WithMany(match => match.Players).HasForeignKey(player => player.TeamId).HasPrincipalKey(match => match.Id).OnDelete(DeleteBehavior.NoAction);
            builder.HasOne(player => player.Season).WithMany(team => team.Players).HasForeignKey(player => player.SeasonId).HasPrincipalKey(team => team.Id).OnDelete(DeleteBehavior.NoAction);

            builder.Property(player => player.DateCreated).IsRequired().HasDefaultValueSql("GETUTCDATE()");
        }
    }
}
