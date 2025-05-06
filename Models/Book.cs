using System.ComponentModel.DataAnnotations;

namespace OnlineBookStore.Models
{
    public enum BookFormat
    {
        Paperback,
        Hardcover,
        Signed,
        Limited,
        First,
        Collector,
        Deluxe
    }

    public class Book
    {
        public int BookId { get; set; }

        [Required]
        [StringLength(20)]
        public string ISBN { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        [StringLength(100)]
        public string Author { get; set; }

        public string Description { get; set; }

        [Required]
        [StringLength(50)]
        public string Genre { get; set; }

        [StringLength(100)]
        public string Publisher { get; set; }

        [Required]
        [StringLength(30)]
        public string Language { get; set; }

        [Required]
        public BookFormat Format { get; set; }

        [Required]
        [Range(0, 10000)]
        public decimal Price { get; set; }

        public int StockQuantity { get; set; } = 0;

        [Range(0, 5)]
        public decimal AverageRating { get; set; } = 0.00m;

        public DateTime? PublicationDate { get; set; }

        public DateTime AddedDate { get; set; } = DateTime.UtcNow;

        public bool IsOnSale { get; set; } = false;

        [StringLength(255)]
        public string CoverImageUrl { get; set; }
    }
}