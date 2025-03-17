using Infrastructure.Entity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public class MyDbContext : DbContext {
    private string DbPath { get; } = GenerateStoragePath();

    public DbSet<User> Users { get; set; }
    public DbSet<Light> Lights { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options) {
        options.UseSqlite($"Data Source={DbPath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        base.OnModelCreating(modelBuilder);

        //modelBuilder.Entity<Light>()
        //    .HasMany(l => l.MotionHistory)
        //    .WithOne(m => m.Light)
        //    .HasForeignKey(m => m.LightId)
        //    .HasPrincipalKey(l => l.Id);

    }

    private static string GenerateStoragePath() {
#if RELEASE
                var path = "/data/";
#else
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
#endif

        if (!Directory.Exists(path)) {
            Directory.CreateDirectory(path);
        }

        return System.IO.Path.Join(path, "COEN390.db");
    }
}
