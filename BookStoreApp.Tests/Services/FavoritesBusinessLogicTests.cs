using BookStoreApp.Data;
using BookStoreApp.Models;
using Microsoft.EntityFrameworkCore;

namespace BookStoreApp.Tests.Services
{
    public class FavoritesBusinessLogicTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly AppUser _testUser;
        private readonly List<Book> _testBooks;

        public FavoritesBusinessLogicTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _context.Database.EnsureCreated();

            _testUser = new AppUser { Id = "testuser1", UserName = "testuser", Name = "Test User" };
            _testBooks = new List<Book>
            {
                new() { Title = "Favorite Book 1", Author = "Author 1", Genre = "Fiction", Price = 15.99m, Stock = 5 },
                new() { Title = "Favorite Book 2", Author = "Author 2", Genre = "Romance", Price = 18.99m, Stock = 3 },
                new() { Title = "Favorite Book 3", Author = "Author 3", Genre = "Science", Price = 22.99m, Stock = 8 }
            };

            _context.Users.Add(_testUser);
            _context.Books.AddRange(_testBooks);
            _context.SaveChanges();
        }

        [Fact]
        public async Task ToggleFavorite_WhenNotFavorite_ShouldAddToFavorites()
        {
            var bookId = _testBooks[0].Id;

            var existingFavorite = await _context.FavouriteBooks
                .FirstOrDefaultAsync(f => f.UserId == _testUser.Id && f.BookId == bookId);

            if (existingFavorite == null)
            {
                var newFavorite = new FavouriteBook
                {
                    UserId = _testUser.Id,
                    BookId = bookId
                };

                await _context.FavouriteBooks.AddAsync(newFavorite);
                await _context.SaveChangesAsync();
            }

            var savedFavorite = await _context.FavouriteBooks
                .FirstOrDefaultAsync(f => f.UserId == _testUser.Id && f.BookId == bookId);

            Assert.NotNull(savedFavorite);
            Assert.Equal(_testUser.Id, savedFavorite.UserId);
            Assert.Equal(bookId, savedFavorite.BookId);
        }

        [Fact]
        public async Task ToggleFavorite_WhenAlreadyFavorite_ShouldRemoveFromFavorites()
        {
            var favorite = new FavouriteBook
            {
                UserId = _testUser.Id,
                BookId = _testBooks[0].Id
            };

            await _context.FavouriteBooks.AddAsync(favorite);
            await _context.SaveChangesAsync();

            var existingFavorite = await _context.FavouriteBooks
                .FirstOrDefaultAsync(f => f.UserId == _testUser.Id && f.BookId == _testBooks[0].Id);

            if (existingFavorite != null)
            {
                _context.FavouriteBooks.Remove(existingFavorite);
                await _context.SaveChangesAsync();
            }

            var removedFavorite = await _context.FavouriteBooks
                .FirstOrDefaultAsync(f => f.UserId == _testUser.Id && f.BookId == _testBooks[0].Id);

            Assert.Null(removedFavorite);
        }

        [Fact]
        public async Task GetUserFavorites_ShouldReturnFavoritesWithBookDetails()
        {
            var favorites = new List<FavouriteBook>
            {
                new() { UserId = _testUser.Id, BookId = _testBooks[0].Id },
                new() { UserId = _testUser.Id, BookId = _testBooks[2].Id },
                new() { UserId = "otheruser", BookId = _testBooks[1].Id } // Different user
            };

            await _context.FavouriteBooks.AddRangeAsync(favorites);
            await _context.SaveChangesAsync();

            var userFavorites = await _context.FavouriteBooks
                .Where(f => f.UserId == _testUser.Id)
                .Include(f => f.Book)
                .Select(f => new
                {
                    BookId = f.BookId,
                    Title = f.Book.Title,
                    Author = f.Book.Author,
                    Genre = f.Book.Genre,
                    Price = f.Book.Price
                })
                .OrderBy(f => f.Title)
                .ToListAsync();

            Assert.Equal(2, userFavorites.Count);
            Assert.Equal("Favorite Book 1", userFavorites[0].Title);
            Assert.Equal("Favorite Book 3", userFavorites[1].Title);
        }

        [Fact]
        public async Task CheckFavoriteStatus_ShouldReturnCorrectStatus()
        {
            var favorite = new FavouriteBook
            {
                UserId = _testUser.Id,
                BookId = _testBooks[0].Id
            };

            await _context.FavouriteBooks.AddAsync(favorite);
            await _context.SaveChangesAsync();

            var isFavoriteBook1 = await _context.FavouriteBooks
                .AnyAsync(f => f.UserId == _testUser.Id && f.BookId == _testBooks[0].Id);

            var isFavoriteBook2 = await _context.FavouriteBooks
                .AnyAsync(f => f.UserId == _testUser.Id && f.BookId == _testBooks[1].Id);

            Assert.True(isFavoriteBook1);
            Assert.False(isFavoriteBook2);
        }

        [Fact]
        public async Task GetFavoritesByGenre_ShouldFilterCorrectly()
        {
            var favorites = new List<FavouriteBook>
            {
                new() { UserId = _testUser.Id, BookId = _testBooks[0].Id }, // Fiction
                new() { UserId = _testUser.Id, BookId = _testBooks[1].Id }, // Romance
                new() { UserId = _testUser.Id, BookId = _testBooks[2].Id }  // Science
            };

            await _context.FavouriteBooks.AddRangeAsync(favorites);
            await _context.SaveChangesAsync();

            var fictionFavorites = await _context.FavouriteBooks
                .Where(f => f.UserId == _testUser.Id)
                .Include(f => f.Book)
                .Where(f => f.Book.Genre == "Fiction")
                .ToListAsync();

            var romanceFavorites = await _context.FavouriteBooks
                .Where(f => f.UserId == _testUser.Id)
                .Include(f => f.Book)
                .Where(f => f.Book.Genre == "Romance")
                .ToListAsync();

            Assert.Single(fictionFavorites);
            Assert.Equal("Favorite Book 1", fictionFavorites[0].Book.Title);

            Assert.Single(romanceFavorites);
            Assert.Equal("Favorite Book 2", romanceFavorites[0].Book.Title);
        }

        [Fact]
        public async Task AddToCartFromFavorites_ShouldMoveBookToCart()
        {
            var favorite = new FavouriteBook
            {
                UserId = _testUser.Id,
                BookId = _testBooks[0].Id
            };

            await _context.FavouriteBooks.AddAsync(favorite);
            await _context.SaveChangesAsync();

            var favoriteBook = await _context.FavouriteBooks
                .Include(f => f.Book)
                .FirstOrDefaultAsync(f => f.UserId == _testUser.Id && f.BookId == _testBooks[0].Id);

            if (favoriteBook != null)
            {
                var existingCartItem = await _context.CartItems
                    .FirstOrDefaultAsync(c => c.UserId == _testUser.Id && c.BookId == favoriteBook.BookId);

                if (existingCartItem == null)
                {
                    var cartItem = new CartItem
                    {
                        UserId = _testUser.Id,
                        BookId = favoriteBook.BookId,
                        Quantity = 1
                    };

                    await _context.CartItems.AddAsync(cartItem);
                }
                else
                {
                    existingCartItem.Quantity += 1;
                    _context.CartItems.Update(existingCartItem);
                }

                await _context.SaveChangesAsync();
            }

            var addedCartItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.UserId == _testUser.Id && c.BookId == _testBooks[0].Id);

            var favoritesStillExists = await _context.FavouriteBooks
                .AnyAsync(f => f.UserId == _testUser.Id && f.BookId == _testBooks[0].Id);

            Assert.NotNull(addedCartItem);
            Assert.Equal(1, addedCartItem.Quantity);
            Assert.True(favoritesStillExists);
        }

        [Fact]
        public async Task GetFavoritesCount_ShouldReturnCorrectCount()
        {
            var favorites = new List<FavouriteBook>
            {
                new() { UserId = _testUser.Id, BookId = _testBooks[0].Id },
                new() { UserId = _testUser.Id, BookId = _testBooks[1].Id },
                new() { UserId = _testUser.Id, BookId = _testBooks[2].Id },
                new() { UserId = "otheruser", BookId = _testBooks[0].Id } // Different user
            };

            await _context.FavouriteBooks.AddRangeAsync(favorites);
            await _context.SaveChangesAsync();

            var userFavoritesCount = await _context.FavouriteBooks
                .CountAsync(f => f.UserId == _testUser.Id);

            Assert.Equal(3, userFavoritesCount);
        }

        [Fact]
        public async Task RemoveAllFavorites_ShouldClearUserFavorites()
        {
            var favorites = new List<FavouriteBook>
            {
                new() { UserId = _testUser.Id, BookId = _testBooks[0].Id },
                new() { UserId = _testUser.Id, BookId = _testBooks[1].Id },
                new() { UserId = "otheruser", BookId = _testBooks[2].Id } // Different user
            };

            await _context.FavouriteBooks.AddRangeAsync(favorites);
            await _context.SaveChangesAsync();

            var userFavorites = await _context.FavouriteBooks
                .Where(f => f.UserId == _testUser.Id)
                .ToListAsync();

            _context.FavouriteBooks.RemoveRange(userFavorites);
            await _context.SaveChangesAsync();

            var remainingUserFavorites = await _context.FavouriteBooks
                .Where(f => f.UserId == _testUser.Id)
                .ToListAsync();

            var otherUserFavorites = await _context.FavouriteBooks
                .Where(f => f.UserId == "otheruser")
                .ToListAsync();

            Assert.Empty(remainingUserFavorites);
            Assert.Single(otherUserFavorites);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
