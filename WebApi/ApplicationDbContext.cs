using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApi.Data.Entities;

namespace WebApi;

public class ApplicationDbContext : IdentityDbContext<IdentityUser>
{
    public DbSet<Data.Entities.Monitor> Monitors => Set<Data.Entities.Monitor>();
    public DbSet<MonitorCheck> MonitorChecks => Set<MonitorCheck>();

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Data.Entities.Monitor>(entity =>
        {
            entity.HasOne(m => m.User)
                  .WithMany()
                  .HasForeignKey(m => m.UserId)
                  .IsRequired();

            entity.HasMany(m => m.Checks)
                  .WithOne(c => c.Monitor)
                  .HasForeignKey(c => c.MonitorId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.Property(m => m.Port);
        });
    }
}
