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

        // GET /api/moods?year=2026&month=6&day=29
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? year, [FromQuery] int? month, [FromQuery] int? day)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                return Unauthorized(new { message = $"無法辨識使用者" });
            }

            var userId = int.Parse(userIdClaim);

            // 根據使用者 ID 來篩選資料
            var query = _context.MoodEntries.Where(m => m.UserId == userId);

            // 根據 query string 的 year, month, day 來篩選資料
            if (year.HasValue)
            {
                query = query.Where(m => m.RecordDate.Year == year.Value);
            }
            if (month.HasValue)
            {
                query = query.Where(m => m.RecordDate.Month == month.Value);
            }
            if (day.HasValue)
            {
                query = query.Where(m => m.RecordDate.Day == day.Value);
            }

            // 依 RecordDate 排序
            var moods = await query.OrderBy(m => m.RecordDate).ToListAsync();
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
            // 限定這五種 emoji
            var allowedMoods = new[] { "😄", "😊", "😐", "😟", "😭" };
            if (!allowedMoods.Contains(request.MoodType))
            {
                return BadRequest(new { message = "MoodType 只能是 😄 😊 😐 😟 😭 其中一種" });
            }

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

        // POST /api/moods/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateMoodRequest request)
        {
            var allowedMoods = new[] { "😄", "😊", "😐", "😟", "😭" };
            if (!allowedMoods.Contains(request.MoodType))
            {
                return BadRequest(new { message = "MoodType 只能是 😄 😊 😐 😟 😭 其中一種" });
            }

            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value; // 從 JWT Token 的 Claims 讀取 userId
            if (userIdClaim == null)
            {
                return Unauthorized(new { message = $"無法辨識使用者" });
            }
            var userId = int.Parse(userIdClaim);

            var mood = await _context.MoodEntries.FindAsync(id);
            if (mood == null)
            {
                return NotFound(new { message = $"找不到 id={id} 的紀錄" });
            }

            // 確認這筆紀錄是屬於該使用者的
            if (mood.UserId != userId)
            {
                return Forbid();
            }

            mood.MoodType = request.MoodType;
            mood.Content = request.Content;

            await _context.SaveChangesAsync();

            return Ok(mood);
        }

        // DELETE /api/moods/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                return Unauthorized(new { message = $"無法辨識使用者" });
            }
            var userId = int.Parse(userIdClaim);

            var mood = await _context.MoodEntries.FindAsync(id);
            if (mood == null)
            {
                return NotFound(new { message = $"找不到 id={id} 的紀錄" });
            }

            if (mood.UserId != userId)
            {
                return Forbid();
            }

            _context.MoodEntries.Remove(mood);
            await _context.SaveChangesAsync();

            return NoContent();
        }

    }


    public class CreateMoodRequest
    {
        public string MoodType { get; set; } = string.Empty;
        public string? Content { get; set; }
    }

    public class UpdateMoodRequest
    {
        public string MoodType { get; set; } = string.Empty;
        public string? Content { get; set; }
    }
}

