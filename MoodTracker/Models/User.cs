using System.ComponentModel.DataAnnotations;

namespace MoodTracker.Models
{
    public class User
    {
        public int Id { get; set; }
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;
        [MaxLength(255)]
        public string Password { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // 一個 User 可以有很多 MoodEntry (紀錄)
        public List<MoodEntry> MoodEntries { get; set; } = new List<MoodEntry>();
    }
}
