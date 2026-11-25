using haobtPC11458_ASM01.Models;
using Microsoft.EntityFrameworkCore;

namespace haobtPC11458_ASM01.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Character> Characters => Set<Character>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Username).HasMaxLength(50).IsRequired();
            entity.HasIndex(a => a.Username).IsUnique();
            entity.Property(a => a.PasswordHash).HasMaxLength(255).IsRequired();
            entity.Property(a => a.CreatedAt).IsRequired();
        });

        modelBuilder.Entity<Character>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).HasMaxLength(50).IsRequired();
            entity.Property(c => c.Class).HasMaxLength(30).IsRequired();
            entity.Property(c => c.Level).HasDefaultValue(1).IsRequired();
            entity.Property(c => c.Hp).HasDefaultValue(100).IsRequired();
            entity.Property(c => c.CreatedAt).IsRequired();

            entity.HasOne(c => c.Account)
                  .WithMany(a => a.Characters)
                  .HasForeignKey(c => c.AccountId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}