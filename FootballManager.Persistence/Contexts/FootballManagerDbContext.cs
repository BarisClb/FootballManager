using FootballManager.Domain.Entities;
using FootballManager.Domain.Settings;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace FootballManager.Persistence.Contexts
{
    public class FootballManagerDbContext : DbContext
    {
        public FootballManagerDbContext(DbContextOptions options) : base(options)
        {
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }


        public DbSet<Goal> Goals { get; set; }
        public DbSet<Match> Matches { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<Season> Seasons { get; set; }
        public DbSet<SeasonPass> SeasonPasses { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<User> Users { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetAssembly(typeof(UserSettings)));
        }

        public override int SaveChanges()
        {
            var datas = ChangeTracker.Entries<BaseEntity>();

            foreach (var data in datas)
            {
                if (data.State == EntityState.Added)
                    data.Entity.DateCreated = DateTime.UtcNow;
                else if (data.State == EntityState.Modified)
                    data.Entity.DateUpdated = DateTime.UtcNow;
            }
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var datas = ChangeTracker.Entries<BaseEntity>();

            foreach (var data in datas)
            {
                if (data.State == EntityState.Added)
                    data.Entity.DateCreated = DateTime.UtcNow;
                else if (data.State == EntityState.Modified)
                    data.Entity.DateUpdated = DateTime.UtcNow;
            }
            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
