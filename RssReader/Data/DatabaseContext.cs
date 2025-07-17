using Microsoft.EntityFrameworkCore;
using RssReader.Models;
using System.IO;

namespace RssReader.Data
{
    public class DatabaseContext : DbContext
    {
        public DbSet<Source> Sources { get; set; }
        public DbSet<Article> Articles { get; set; }
        public DbSet<Settings> Settings { get; set; }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string dbPath = Path.Combine(
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData),
                "RssReader",
                "rssreader.db");
                
            // Ensure directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(dbPath));
            
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure cascade delete for Source -> Articles
            modelBuilder.Entity<Source>()
                .HasMany(s => s.Articles)
                .WithOne(a => a.Source)
                .HasForeignKey(a => a.SourceId)
                .OnDelete(DeleteBehavior.Cascade);
                
            // Add default settings if none exist
            modelBuilder.Entity<Settings>().HasData(
                new Settings { Id = 1 }
            );
        }
    }
}