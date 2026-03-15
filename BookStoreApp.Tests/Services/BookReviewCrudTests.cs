using BookStoreApp.Data;
using BookStoreApp.Models;
using Microsoft.EntityFrameworkCore;

namespace BookStoreApp.Tests.Services
{
    public class BookReviewCrudTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly AppUser _testUser;
        private readonly Book _testBook;

        public BookReviewCrudTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _context.Database.EnsureCreated();

            _testUser = new AppUser { Id = "testuser1", UserName = "testuser", Name = "Test User" };
            _testBook = new Book { Title = "Review Test Book", Author = "Review Author", Genre = "Fiction", Price = 15.99m, Stock = 5 };

            _context.Users.Add(_testUser);
            _context.Books.Add(_testBook);
            _context.SaveChanges();
        }

        [Fact]
        public async Task CreateReview_ShouldAddReviewToDatabase()
        {

            var review = new BookReview
            {
                BookId = _testBook.Id,
                UserId = _testUser.Id,
                Rating = 5,
                Comment = "Excellent book! Highly recommended.",
                CreatedAt = DateTime.UtcNow
            };


            await _context.BookReviews.AddAsync(review);
            await _context.SaveChangesAsync();


            var savedReview = await _context.BookReviews.FindAsync(review.Id);
            Assert.NotNull(savedReview);
            Assert.Equal(_testBook.Id, savedReview.BookId);
            Assert.Equal(_testUser.Id, savedReview.UserId);
            Assert.Equal(5, savedReview.Rating);
            Assert.Equal("Excellent book! Highly recommended.", savedReview.Comment);
        }

        [Fact]
        public async Task UpdateReview_ShouldModifyExistingReview()
        {

            var review = new BookReview
            {
                BookId = _testBook.Id,
                UserId = _testUser.Id,
                Rating = 3,
                Comment = "It was okay.",
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            };

            await _context.BookReviews.AddAsync(review);
            await _context.SaveChangesAsync();


            review.Rating = 5;
            review.Comment = "Actually, it was good";
            _context.BookReviews.Update(review);
            await _context.SaveChangesAsync();


            var updatedReview = await _context.BookReviews.FindAsync(review.Id);
            Assert.NotNull(updatedReview);
            Assert.Equal(5, updatedReview.Rating);
            Assert.Equal("Actually, it was good", updatedReview.Comment);
        }

        [Fact]
        public async Task DeleteReview_ShouldRemoveReviewFromDatabase()
        {

            var review = new BookReview
            {
                BookId = _testBook.Id,
                UserId = _testUser.Id,
                Rating = 4,
                Comment = "Good book to delete.",
                CreatedAt = DateTime.UtcNow
            };

            await _context.BookReviews.AddAsync(review);
            await _context.SaveChangesAsync();
            var reviewId = review.Id;


            _context.BookReviews.Remove(review);
            await _context.SaveChangesAsync();


            var deletedReview = await _context.BookReviews.FindAsync(reviewId);
            Assert.Null(deletedReview);
        }

        [Fact]
        public async Task CalculateAverageRating_ShouldReturnCorrectAverage()
        {

            var reviews = new List<BookReview>
            {
                new() { BookId = _testBook.Id, UserId = _testUser.Id, Rating = 5, Comment = "Great!", CreatedAt = DateTime.UtcNow.AddDays(-3) },
                new() { BookId = _testBook.Id, UserId = "user2", Rating = 4, Comment = "Good", CreatedAt = DateTime.UtcNow.AddDays(-2) },
                new() { BookId = _testBook.Id, UserId = "user3", Rating = 3, Comment = "Average", CreatedAt = DateTime.UtcNow.AddDays(-1) }
            };

            await _context.BookReviews.AddRangeAsync(reviews);
            await _context.SaveChangesAsync();


            var averageRating = await _context.BookReviews
                .Where(r => r.BookId == _testBook.Id)
                .AverageAsync(r => r.Rating);


            Assert.Equal(4.0, averageRating); 
        }

        [Fact]
        public async Task GetUserReviewForBook_ShouldReturnSpecificUserReview()
        {

            var review = new BookReview
            {
                BookId = _testBook.Id,
                UserId = _testUser.Id,
                Rating = 5,
                Comment = "My personal review.",
                CreatedAt = DateTime.UtcNow
            };

            await _context.BookReviews.AddAsync(review);
            await _context.SaveChangesAsync();


            var userReview = await _context.BookReviews
                .FirstOrDefaultAsync(r => r.BookId == _testBook.Id && r.UserId == _testUser.Id);


            Assert.NotNull(userReview);
            Assert.Equal(_testUser.Id, userReview.UserId);
            Assert.Equal(_testBook.Id, userReview.BookId);
            Assert.Equal("My personal review.", userReview.Comment);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
