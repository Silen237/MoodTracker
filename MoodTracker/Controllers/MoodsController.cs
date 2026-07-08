using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoodTracker.Data;
using MoodTracker.Models;
using MoodTracker.DTOs;

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
            var moods = await query.OrderBy(m => m.RecordDate).ToListAsync();

            var result = moods.Select(m => new MoodResponse
            {
                Id = m.Id,
                MoodType = m.MoodType,
                Content = m.Content,
                Tags = m.Tags,
                RecordDate = m.RecordDate
            });

            return Ok(result);
        }

        // GET /api/moods/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var mood = await _context.MoodEntries.FindAsync(id);
            if (mood == null)
                return NotFound(new { message = $"找不到 id={id} 的紀錄" });

            var result = new MoodResponse
            {
                Id = mood.Id,
                MoodType = mood.MoodType,
                Content = mood.Content,
                Tags = mood.Tags,
                RecordDate = mood.RecordDate
            };

            return Ok(result);
        }

        // 取得使用者的心情年/月統計資料
        // GET /api/moods/states
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats([FromQuery] int? year, [FromQuery] int? month)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                return Unauthorized(new { massage =$"無法辨識使用者" });
            }
            var userId = int.Parse(userIdClaim);

            var query = _context.MoodEntries.Where(m => m.UserId == userId);

            if (year.HasValue)
            {
                query = query.Where(m => m.RecordDate.Year == year.Value);
            }
            if (month.HasValue)
            {
                query = query.Where(m => m.RecordDate.Month == month.Value);
            }

            var moods = await query.ToListAsync();

            var allowedMoods = new[] { "😄", "😊", "😐", "😟", "😭" };

            // 計算每種 emoji 的數量
            var counts = allowedMoods.ToDictionary(
                emoji => emoji,
                emoji => moods.Count(m => m.MoodType == emoji)
            );

            var maxCount = counts.Max(x => x.Value);

            string mostFrequent;
            if (maxCount == 0)
            {
                mostFrequent = "尚無紀錄";
            }
            else if (counts.Count(x => x.Value == maxCount) == counts.Count)
            {
                mostFrequent = "五個心情狀態一樣多";
            }
            else if (counts.Count(x => x.Value == maxCount) > 1)
            {
                mostFrequent = "目前有多個心情並列最多";
            }
            else
            {
                mostFrequent = counts.First(x => x.Value == maxCount).Key;
            }

            return Ok(new
            {
                total = moods.Count,
                counts = counts,
                mostFrequent = mostFrequent
            });
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

            if (!string.IsNullOrWhiteSpace(request.Tags))
            {
                var tagList = request.Tags.Split(',').Select(t => t.Trim()).ToList();
                foreach (var tag in tagList)
                {
                    if (tag.Length > 10)
                    {
                        return BadRequest(new { message = $"標籤 '{tag}' 長度超過 10 個字元" });
                    }
                }
            }

            var newMood = new MoodEntry
            {
                UserId = userId,
                MoodType = request.MoodType,
                Tags = request.Tags,
                Content = request.Content,
                RecordDate = DateOnly.FromDateTime(DateTime.Today)
            };

            _context.MoodEntries.Add(newMood);
            Console.WriteLine($"Trying to insert MoodEntry with UserId: {userId}");
            await _context.SaveChangesAsync();

            var result = new MoodResponse
            {
                Id = newMood.Id,
                MoodType = newMood.MoodType,
                Content = newMood.Content,
                Tags = newMood.Tags,
                RecordDate = newMood.RecordDate
            };

            return CreatedAtAction(nameof(GetById), new { id = newMood.Id }, result);
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

            if (!string.IsNullOrWhiteSpace(request.Tags))
            {
                var tagList = request.Tags.Split(',').Select(t => t.Trim()).ToList();
                foreach (var tag in tagList)
                {
                    if (tag.Length > 10)
                    {
                        return BadRequest(new { message = $"標籤 '{tag}' 長度超過 10 個字元" });
                    }
                }
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
            mood.Tags = request.Tags;

            await _context.SaveChangesAsync();

            var result = new MoodResponse
            {
                Id = mood.Id,
                MoodType = mood.MoodType,
                Content = mood.Content,
                Tags = mood.Tags,
                RecordDate = mood.RecordDate
            };

            return Ok(result);
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
        public string? Tags { get; set; }
    }

    public class UpdateMoodRequest
    {
        public string MoodType { get; set; } = string.Empty;
        public string? Content { get; set; }
        public string? Tags { get; set; }
    }
}

