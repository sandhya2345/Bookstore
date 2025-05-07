using System.ComponentModel.DataAnnotations;

namespace OnlineBookStore.Models
{
    public enum UserRole
    {
        Member,
        Staff,
        Admin
    }
    public class User
    {
        public int UserId { get; set; }

        [Required]
        [StringLength(50)]
        public string? Username { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        
        public string Email { get; set; }

        [Required]
        public string? PasswordHash { get; set; }

        [Required]
        [StringLength(50)]
        public string? FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string? LastName { get; set; }

        [Required]
        public UserRole Role { get; set; } = UserRole.Member;

        public DateTime JoinDate { get; set; } = DateTime.UtcNow;

        public int TotalOrders { get; set; } = 0;

        public bool IsActive { get; set; } = true;


        public ICollection<Announcement>? Announcements { get; set; }
    }
}