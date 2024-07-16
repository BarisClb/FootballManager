using FootballManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballManager.Domain.Settings
{
    public class SeasonSettingsSettings : IEntityTypeConfiguration<Domain.Entities.SeasonSettings>
    {
        public void Configure(EntityTypeBuilder<Domain.Entities.SeasonSettings> builder)
        {
            builder.ToTable("SeasonSettings").HasKey(seasonSettings => seasonSettings.Id);

            builder.Property(seasonSettings => seasonSettings.SeasonId).IsRequired();
            builder.Property(seasonSettings => seasonSettings.MatchTimes);
            builder.Property(seasonSettings => seasonSettings.MatchIntervalMinutes);
            builder.Property(seasonSettings => seasonSettings.LeagueHalvesMinumumIntervalHours);
            builder.Property(seasonSettings => seasonSettings.HomeTeamExtraGoalChance);
            builder.Property(seasonSettings => seasonSettings.TiebreakerMatchHoursAfter);
            builder.Property(seasonSettings => seasonSettings.StartNextRoundNextDay);
            builder.Property(seasonSettings => seasonSettings.AllowNoAssistGoals);
            builder.Property(seasonSettings => seasonSettings.NumberOfTeamsToParticipate);
            builder.Property(seasonSettings => seasonSettings.SeasonStartDay);

            builder.HasOne(seasonSettings => seasonSettings.Season).WithOne(season => season.SeasonSettings).HasForeignKey<Domain.Entities.SeasonSettings>(seasonSettings => seasonSettings.SeasonId).HasPrincipalKey<Domain.Entities.Season>(season => season.Id).OnDelete(DeleteBehavior.NoAction);

            builder.Property(seasonSettings => seasonSettings.DateCreated).IsRequired().HasDefaultValueSql("GETUTCDATE()");
        }
    }
}
