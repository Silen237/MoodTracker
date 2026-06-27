using System.ComponentModel.DataAnnotations;

namespace MoodTracker.Models
{
    public class MoodEntry
    {
        public int Id { get; set; } // 主鍵，資料庫會自動生成
        public int UserId { get; set; } // 外來鍵，對應到 User 的 Id
        [MaxLength(20)]
        public string MoodType { get; set; } = string.Empty; // 不可空，預設為空字串""
        [MaxLength(1000)]
        public string? Content {get; set; } // 可空，預設為 null
        [MaxLength(100)]
        public string? Tags { get; set; } 
        public DateOnly RecordDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // 這筆 MoodEntry (紀錄) 屬於哪個 User
        public User User { get; set; } = null!;
    }
}
