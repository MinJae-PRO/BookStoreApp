using BookStoreApp.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
// Implements Identtiy DbContext to manage user authentication and authorization in the application.
namespace BookStoreApp.Data
{ 
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Book> Books { get; set; } = default!;
        public DbSet<FavouriteBook> FavouriteBooks { get; set; } = default!;
        public DbSet<BookReview> BookReviews { get; set; } = default!;
        public DbSet<CartItem> CartItems { get; set; } = default!;
        public DbSet<Order> Orders { get; set; } = default!;
        public DbSet<OrderItem> OrderItems { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Book>()
                .Property(b => b.Price)
                .HasPrecision(18, 2);

            // Prevent duplicate favorites per user
            modelBuilder.Entity<FavouriteBook>()
                .HasIndex(f => new { f.UserId, f.BookId })
                .IsUnique();

            // One review per user per book
            modelBuilder.Entity<BookReview>()
                .HasIndex(r => new { r.UserId, r.BookId })
                .IsUnique();

            // One cart item per user per book
            modelBuilder.Entity<CartItem>()
                .HasIndex(c => new { c.UserId, c.BookId })
                .IsUnique();

            modelBuilder.Entity<Order>()
                .Property(o => o.TotalPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.UnitPrice)
                .HasPrecision(18, 2);
        }
    }
}