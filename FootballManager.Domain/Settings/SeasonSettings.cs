using FootballManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FootballManager.Domain.Settings
{
    public class SeasonSettings : IEntityTypeConfiguration<Season>
    {
        public void Configure(EntityTypeBuilder<Season> builder)
        {
            builder.ToTable("Seasons").HasKey(season => season.Id);

            builder.Property(season => season.Name).IsRequired();
            builder.Property(season => season.SeasonType);
            builder.Property(season => season.IsRegistrationOpen).IsRequired().HasDefaultValueSql("1");
            builder.Property(season => season.HasSeasonEnded).IsRequired().HasDefaultValueSql("0");

            builder.HasOne(season => season.Champion).WithMany(team => team.ChampionSeasons).HasForeignKey(season => season.ChampionId).HasPrincipalKey(team => team.Id).OnDelete(DeleteBehavior.NoAction);
            builder.HasOne(season => season.SeasonSettings).WithOne(seasonSettings => seasonSettings.Season).HasForeignKey<Domain.Entities.Season>(season => season.SeasonSettingsId).HasPrincipalKey<Domain.Entities.SeasonSettings>(seasonSettings => seasonSettings.Id).OnDelete(DeleteBehavior.NoAction);

            builder.Property(season => season.DateCreated).IsRequired().HasDefaultValueSql("GETUTCDATE()");
        }
    }
}
