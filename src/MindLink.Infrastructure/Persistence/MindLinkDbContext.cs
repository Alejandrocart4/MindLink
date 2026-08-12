using Microsoft.EntityFrameworkCore;
using MindLink.Domain.Entities;

namespace MindLink.Infrastructure.Persistence;

public sealed class MindLinkDbContext(DbContextOptions<MindLinkDbContext> options) : DbContext(options)
{
    public DbSet<LocalUser> Users => Set<LocalUser>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LocalUser>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FullName).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(160).IsRequired();
            entity.HasIndex(x => x.Email).IsUnique();
            entity.Property(x => x.PasswordHash).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Plan).HasMaxLength(20).IsRequired();
        });

        modelBuilder.Entity<ActivityLog>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Title).HasMaxLength(160).IsRequired();
            entity.Property(x => x.Detail).HasMaxLength(300).IsRequired();
            entity.Property(x => x.Category).HasMaxLength(40).IsRequired();
        });
    }
}
