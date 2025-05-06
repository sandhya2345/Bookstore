using System.ComponentModel.DataAnnotations;

namespace OnlineBookStore.Models
{
    public class Announcement
    {
        public int AnnouncementId { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; } = true;

        public int CreatedBy { get; set; }
        public User Creator { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}