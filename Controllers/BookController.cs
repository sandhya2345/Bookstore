using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineBookStore.Models;
using OnlineBookStore.ViewModels;
using System.Threading.Tasks;

namespace OnlineBookStore.Controllers
{
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BooksController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Books/Browse
        public IActionResult Browse(string searchTerm, string genre, string sortBy, int page = 1)
        {
            int pageSize = 9;
            var query = _context.Books.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
                query = query.Where(b => b.Title.Contains(searchTerm) || b.Author.Contains(searchTerm));

            if (!string.IsNullOrEmpty(genre))
                query = query.Where(b => b.Genre == genre);

            if (!string.IsNullOrEmpty(sortBy))
            {
                switch (sortBy.ToLower())
                {
                    case "price_asc":
                        query = query.OrderBy(b => b.Price);
                        break;
                    case "price_desc":
                        query = query.OrderByDescending(b => b.Price);
                        break;
                    case "title":
                        query = query.OrderBy(b => b.Title);
                        break;
                }
            }

            var totalBooks = query.Count();
            var books = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var genres = _context.Books.Select(b => b.Genre).Distinct().ToList();

            var viewModel = new BookBrowseViewModel
            {
                Books = books,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalBooks / (double)pageSize),
                SearchTerm = searchTerm,
                Genre = genre,
                SortBy = sortBy,
                Genres = genres
            };

            return View(viewModel); 
        }


        // GET: /Books/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var book = await _context.Books.FirstOrDefaultAsync(b => b.BookId == id);
            if (book == null) return NotFound();

            return View(book);
        }
    }
}
