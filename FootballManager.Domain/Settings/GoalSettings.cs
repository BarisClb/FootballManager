using FootballManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FootballManager.Domain.Settings
{
    public class GoalSettings : IEntityTypeConfiguration<Goal>
    {
        public void Configure(EntityTypeBuilder<Goal> builder)
        {
            builder.ToTable("Goals").HasKey(goal => goal.Id);

            builder.Property(goal => goal.MinuteScored).IsRequired();

            builder.HasOne(goal => goal.Match).WithMany(match => match.Goals).HasForeignKey(goal => goal.MatchId).HasPrincipalKey(match => match.Id).OnDelete(DeleteBehavior.NoAction);
            builder.HasOne(goal => goal.TeamScored).WithMany(team => team.Scored).HasForeignKey(goal => goal.TeamScoredId).HasPrincipalKey(team => team.Id).OnDelete(DeleteBehavior.NoAction);
            builder.HasOne(goal => goal.TeamConceded).WithMany(team => team.Conceded).HasForeignKey(goal => goal.TeamConcededId).HasPrincipalKey(team => team.Id).OnDelete(DeleteBehavior.NoAction);
            builder.HasOne(goal => goal.ScoredBy).WithMany(player => player.Scored).HasForeignKey(goal => goal.ScoredById).HasPrincipalKey(player => player.Id).OnDelete(DeleteBehavior.NoAction);
            builder.HasOne(goal => goal.AssistedBy).WithMany(player => player.Assisted).HasForeignKey(goal => goal.AssistedById).HasPrincipalKey(player => player.Id).OnDelete(DeleteBehavior.NoAction);

            builder.Property(goal => goal.DateCreated).IsRequired().HasDefaultValueSql("GETUTCDATE()");
        }
    }
}
