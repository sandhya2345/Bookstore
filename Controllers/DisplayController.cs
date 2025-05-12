using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineBookStore.Models;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OnlineBookStore.Controllers
{
    [Authorize]
    [Route("Books")] 
    public class DisplayController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DisplayController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Books/Display
        [HttpGet("Display")]
        public async Task<IActionResult> Display()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Book)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            // This will look for Views/Books/Display.cshtml
            return View("~/Views/Books/Display.cshtml", orders);
        }

        // Helper to get logged-in user ID
        private int? GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(claim, out var id) ? id : (int?)null;
        }
    }
}
