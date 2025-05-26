using Microsoft.EntityFrameworkCore;
using FileManagementAPI.Models;

namespace FileManagementAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<UploadedFile> UploadedFiles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // User konfigürasyonu
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).HasMaxLength(50);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // UploadedFile konfigürasyonu
        modelBuilder.Entity<UploadedFile>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileName).HasMaxLength(255);
            entity.Property(e => e.OriginalFileName).HasMaxLength(255);
            entity.Property(e => e.ContentType).HasMaxLength(100);
            entity.Property(e => e.FilePath).HasMaxLength(500);

            // İlişki konfigürasyonu
            entity.HasOne(e => e.User)
                  .WithMany(u => u.Files)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        base.OnModelCreating(modelBuilder);
    }
}