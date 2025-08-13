using Domain.Repository.Entities;
using Microsoft.EntityFrameworkCore;


namespace Login.Repository;

public class LoginDbContext : DbContext
{
    public DbSet<User> Users { get; set; }

    public LoginDbContext(DbContextOptions<LoginDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Gender).HasMaxLength(20);
        });
    }
}