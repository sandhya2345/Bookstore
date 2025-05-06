using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineBookStore.Models;
using System;
using System.Linq;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Dashboard");
        }

        // Fetch only 4 featured books that are on sale, ordered by date
        var featuredBooks = _context.Books
            .Where(b => b.IsOnSale == true)
            .OrderByDescending(b => b.AddedDate)
            .Take(4)
            .ToList();

        ViewBag.FeaturedBooks = featuredBooks;

        return View();
    }

    [Authorize]
    public IActionResult Dashboard()
    {
        return View();
    }
}
