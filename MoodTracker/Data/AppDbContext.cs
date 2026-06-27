using Microsoft.EntityFrameworkCore;
using MoodTracker.Models;

namespace MoodTracker.Data
{
    public class AppDbContext : DbContext
    {
        // 建構子，接受 DbContextOptions 參數，並傳給基底類別的建構子
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // 對應 Users 資料表
        public DbSet<User> Users { get; set; }

        // 對應 MoodEntries 資料表
        public DbSet<MoodEntry> MoodEntries { get; set; }

        // 額外設定
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
        }
    }
}
