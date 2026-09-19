using Microsoft.EntityFrameworkCore;
using BlogSite.Models;

namespace BlogSite.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Kullanici> Kullanicilar => Set<Kullanici>();
    public DbSet<Soru> Sorular => Set<Soru>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Post>(entity =>
        {
            entity.ToTable("posts");
            entity.HasIndex(p => p.Slug).IsUnique();
            entity.HasOne(p => p.Yazar)
                  .WithMany()
                  .HasForeignKey(p => p.YazarId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Kullanici>(entity =>
        {
            entity.ToTable("kullanicilar");
            entity.HasIndex(k => k.Email).IsUnique();
        });

        modelBuilder.Entity<Soru>(entity =>
        {
            entity.ToTable("sorular");
            entity.HasOne(s => s.SoranKullanici)
                  .WithMany()
                  .HasForeignKey(s => s.SoranKullaniciId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        base.OnModelCreating(modelBuilder);
    }
}
