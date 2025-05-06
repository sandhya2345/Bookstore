using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineBookStore.Models;

namespace OnlineBookStore.Controllers
{
    public class TestController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TestController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            try
            {
                // Test connection by querying a simple table
                var userCount = _context.Users.Count();
                var bookCount = _context.Books.Count();

                ViewBag.Message = $"Database connection successful! Found {userCount} users and {bookCount} books.";
                ViewBag.IsConnected = true;
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Database connection failed: {ex.Message}";
                ViewBag.IsConnected = false;
            }

            return View();
        }
    }
}