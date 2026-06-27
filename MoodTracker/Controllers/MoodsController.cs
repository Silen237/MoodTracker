using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoodTracker.Data;
using MoodTracker.Models;

namespace MoodTracker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MoodsController : ControllerBase
    {
        
        private readonly AppDbContext _context;
        // 建構子：ASP.NET 會自動把 AppDbContext 傳進來
        public MoodsController(AppDbContext context)
        {
            _context = context;
        }


        // GET /api/moods
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var moods = await _context.MoodEntries.ToListAsync();
            return Ok(moods);
        }

        // GET /api/moods/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var mood = await _context.MoodEntries.FindAsync(id);
            if (mood == null)
                return NotFound(new { message = $"找不到 id={id} 的紀錄" });

            return Ok(mood);
        }

        // POST /api/moods
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateMoodRequest request)
        {

            // 從 JWT Token 的 Claims 讀取 userId
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
                return Unauthorized(new { message = "無法識別使用者" });

            var userId = int.Parse(userIdClaim);

            var newMood = new MoodEntry
            {
                UserId = userId,
                MoodType = request.MoodType,
                Content = request.Content,
                RecordDate = DateOnly.FromDateTime(DateTime.Today)
            };

            _context.MoodEntries.Add(newMood);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = newMood.Id }, newMood);
        }
    }


    public class CreateMoodRequest
    {
        public string MoodType { get; set; } = string.Empty;
        public string? Content { get; set; }
    }
}

