using MoodTracker.Models;
using System.ComponentModel.DataAnnotations;

namespace MoodTracker.DTOs
{
    public class MoodResponse
    {
        public int Id { get; set; }
        public string MoodType { get; set; } = string.Empty; 
        public string? Content { get; set; }
        public string? Tags { get; set; }
        public DateOnly RecordDate { get; set; }
    }
}
