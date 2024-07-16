using FootballManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FootballManager.Domain.Settings
{
    public class UserSettings : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users").HasKey(user => user.Id);

            builder.Property(user => user.Name).IsRequired();
            builder.Property(user => user.Username).IsRequired();
            builder.HasIndex(user => user.Username).IsUnique();
            builder.Property(user => user.Password).IsRequired();
            builder.HasIndex(user => user.Email).IsUnique();
            builder.HasIndex(user => user.Groups).IsUnique();
            builder.HasIndex(user => user.Roles).IsUnique();

            builder.Property(user => user.DateCreated).IsRequired().HasDefaultValueSql("GETUTCDATE()");
        }
    }
}
