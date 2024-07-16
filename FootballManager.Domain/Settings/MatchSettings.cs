using FootballManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FootballManager.Domain.Settings
{
    public class MatchSettings : IEntityTypeConfiguration<Match>
    {
        public void Configure(EntityTypeBuilder<Match> builder)
        {
            builder.ToTable("Matches").HasKey(match => match.Id);

            builder.Property(match => match.MatchType).IsRequired();
            builder.Property(match => match.MatchSeasonPeriodType);
            builder.Property(match => match.SeasonRound);
            builder.Property(match => match.HomeTeamFullTimeScore);
            builder.Property(match => match.AwayTeamFullTimeScore);
            builder.Property(match => match.HomeTeamOvertimeScore);
            builder.Property(match => match.AwayTeamOvertimeScore);
            builder.Property(match => match.HomeTeamPenaltyScore);
            builder.Property(match => match.AwayTeamPenaltyScore);
            builder.Property(match => match.Winner);
            builder.Property(match => match.DatePlanned);
            builder.Property(match => match.DatePlayed);

            builder.HasOne(match => match.Season).WithMany(season => season.Matches).HasForeignKey(match => match.SeasonId).HasPrincipalKey(season => season.Id).OnDelete(DeleteBehavior.NoAction);
            builder.HasOne(match => match.HomeTeam).WithMany(team => team.HomeMatches).HasForeignKey(match => match.HomeTeamId).HasPrincipalKey(team => team.Id).OnDelete(DeleteBehavior.NoAction);
            builder.HasOne(match => match.AwayTeam).WithMany(team => team.AwayMatches).HasForeignKey(match => match.AwayTeamId).HasPrincipalKey(team => team.Id).OnDelete(DeleteBehavior.NoAction);
            builder.HasOne(match => match.HomeTeamManager).WithMany(user => user.HomeMatches).HasForeignKey(match => match.HomeTeamManagerId).HasPrincipalKey(user => user.Id).OnDelete(DeleteBehavior.NoAction);
            builder.HasOne(match => match.AwayTeamManager).WithMany(user => user.AwayMatches).HasForeignKey(match => match.AwayTeamManagerId).HasPrincipalKey(user => user.Id).OnDelete(DeleteBehavior.NoAction);

            builder.Property(match => match.DateCreated).IsRequired().HasDefaultValueSql("GETUTCDATE()");
        }
    }
}
