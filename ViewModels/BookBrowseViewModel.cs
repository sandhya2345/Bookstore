using OnlineBookStore.Models;
using System.Collections.Generic;

namespace OnlineBookStore.ViewModels
{
    public class BookBrowseViewModel
    {
        public IEnumerable<Book> Books { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }

        public string SearchTerm { get; set; }
        public string Genre { get; set; }
        public string SortBy { get; set; }
        public List<string> Genres { get; set; }
    }
}
