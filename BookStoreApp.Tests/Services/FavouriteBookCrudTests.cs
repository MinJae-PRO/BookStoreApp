using BookStoreApp.Data;
using BookStoreApp.Models;
using Microsoft.EntityFrameworkCore;

namespace BookStoreApp.Tests.Services
{
    public class FavouriteBookCrudTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly AppUser _testUser;
        private readonly List<Book> _testBooks;

        public FavouriteBookCrudTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _context.Database.EnsureCreated();

            _testUser = new AppUser { Id = "testuser1", UserName = "testuser", Name = "Test User" };
            _testBooks = new List<Book>
            {
                new() { Title = "Favorite Book 1", Author = "Author 1", Genre = "Fiction", Price = 12.99m, Stock = 5 },
                new() { Title = "Favorite Book 2", Author = "Author 2", Genre = "Romance", Price = 14.99m, Stock = 3 }
            };

            _context.Users.Add(_testUser);
            _context.Books.AddRange(_testBooks);
            _context.SaveChanges();
        }

        [Fact]
        public async Task AddToFavorites_ShouldCreateFavoriteBook()
        {

            var favoriteBook = new FavouriteBook
            {
                UserId = _testUser.Id,
                BookId = _testBooks[0].Id
            };


            await _context.FavouriteBooks.AddAsync(favoriteBook);
            await _context.SaveChangesAsync();


            var savedFavorite = await _context.FavouriteBooks.FindAsync(favoriteBook.Id);
            Assert.NotNull(savedFavorite);
            Assert.Equal(_testUser.Id, savedFavorite.UserId);
            Assert.Equal(_testBooks[0].Id, savedFavorite.BookId);
        }

        [Fact]
        public async Task GetUserFavorites_ShouldReturnUserFavoriteBooks()
        {

            var favorites = new List<FavouriteBook>
            {
                new() { UserId = _testUser.Id, BookId = _testBooks[0].Id },
                new() { UserId = _testUser.Id, BookId = _testBooks[1].Id },
                new() { UserId = "otheruser", BookId = _testBooks[0].Id } // Different user
            };

            await _context.FavouriteBooks.AddRangeAsync(favorites);
            await _context.SaveChangesAsync();


            var userFavorites = await _context.FavouriteBooks
                .Where(f => f.UserId == _testUser.Id)
                .Include(f => f.Book)
                .ToListAsync();


            Assert.Equal(2, userFavorites.Count);
            Assert.All(userFavorites, fav => Assert.Equal(_testUser.Id, fav.UserId));
        }

        [Fact]
        public async Task RemoveFromFavorites_ShouldDeleteFavoriteBook()
        {

            var favoriteBook = new FavouriteBook
            {
                UserId = _testUser.Id,
                BookId = _testBooks[0].Id
            };

            await _context.FavouriteBooks.AddAsync(favoriteBook);
            await _context.SaveChangesAsync();
            var favoriteId = favoriteBook.Id;


            _context.FavouriteBooks.Remove(favoriteBook);
            await _context.SaveChangesAsync();


            var deletedFavorite = await _context.FavouriteBooks.FindAsync(favoriteId);
            Assert.Null(deletedFavorite);
        }

        [Fact]
        public async Task CheckIfBookIsFavorite_ShouldReturnCorrectStatus()
        {

            var favoriteBook = new FavouriteBook
            {
                UserId = _testUser.Id,
                BookId = _testBooks[0].Id
            };

            await _context.FavouriteBooks.AddAsync(favoriteBook);
            await _context.SaveChangesAsync();


            var isFavorite = await _context.FavouriteBooks
                .AnyAsync(f => f.UserId == _testUser.Id && f.BookId == _testBooks[0].Id);

            var isNotFavorite = await _context.FavouriteBooks
                .AnyAsync(f => f.UserId == _testUser.Id && f.BookId == _testBooks[1].Id);


            Assert.True(isFavorite);
            Assert.False(isNotFavorite);
        }

        [Fact]
        public async Task GetFavoriteBooksByGenre_ShouldFilterCorrectly()
        {

            var favorites = new List<FavouriteBook>
            {
                new() { UserId = _testUser.Id, BookId = _testBooks[0].Id }, // Fiction
                new() { UserId = _testUser.Id, BookId = _testBooks[1].Id }  // Romance
            };

            await _context.FavouriteBooks.AddRangeAsync(favorites);
            await _context.SaveChangesAsync();


            var fictionFavorites = await _context.FavouriteBooks
                .Where(f => f.UserId == _testUser.Id)
                .Include(f => f.Book)
                .Where(f => f.Book.Genre == "Fiction")
                .ToListAsync();


            Assert.Single(fictionFavorites);
            Assert.Equal("Fiction", fictionFavorites[0].Book.Genre);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
