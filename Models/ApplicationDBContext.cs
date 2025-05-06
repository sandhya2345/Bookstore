using Microsoft.EntityFrameworkCore;
using System;

namespace OnlineBookStore.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Book> Books { get; set; }

        public DbSet<Order> Orders { get; set; }

        public DbSet<OrderItem> OrderItems { get; set; }

        public DbSet<ShoppingCart> ShoppingCart { get; set; }


        public DbSet<Announcement> Announcements { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure UserRole enum conversion (store as string)
            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasConversion<string>()
                .HasMaxLength(20);

            // Configure BookFormat enum conversion (store as string)
            modelBuilder.Entity<Book>()
                .Property(b => b.Format)
                .HasConversion<string>()
                .HasMaxLength(20);

            // Configure decimal precision
            modelBuilder.Entity<Book>()
                .Property(b => b.Price)
                .HasColumnType("decimal(10,2)");


            modelBuilder.Entity<Announcement>()
                .HasOne(a => a.Creator)
                .WithMany(u => u.Announcements)
                .HasForeignKey(a => a.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}