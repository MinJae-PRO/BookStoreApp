using BookStoreApp.Data;
using BookStoreApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BookStoreApp.Tests.Services
{
    public class CartCrudTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly AppUser _testUser;
        private readonly List<Book> _testBooks;

        public CartCrudTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _context.Database.EnsureCreated();

            _testUser = new AppUser { Id = "testuser1", UserName = "testuser", Name = "Test User" };
            _testBooks = new List<Book>
            {
                new() { Title = "Book 1", Author = "Author 1", Genre = "Fiction", Price = 10.99m, Stock = 5 },
                new() { Title = "Book 2", Author = "Author 2", Genre = "Non-Fiction", Price = 15.99m, Stock = 3 }
            };

            _context.Users.Add(_testUser);
            _context.Books.AddRange(_testBooks);
            _context.SaveChanges();
        }

        [Fact]
        public async Task AddToCart_ShouldCreateCartItem()
        {

            var cartItem = new CartItem
            {
                UserId = _testUser.Id,
                BookId = _testBooks[0].Id,
                Quantity = 2
            };


            await _context.CartItems.AddAsync(cartItem);
            await _context.SaveChangesAsync();


            var savedCartItem = await _context.CartItems.FindAsync(cartItem.Id);
            Assert.NotNull(savedCartItem);
            Assert.Equal(_testUser.Id, savedCartItem.UserId);
            Assert.Equal(_testBooks[0].Id, savedCartItem.BookId);
            Assert.Equal(2, savedCartItem.Quantity);
        }

        [Fact]
        public async Task GetCartItems_ShouldReturnUserCartItems()
        {

            var cartItems = new List<CartItem>
            {
                new() { UserId = _testUser.Id, BookId = _testBooks[0].Id, Quantity = 1 },
                new() { UserId = _testUser.Id, BookId = _testBooks[1].Id, Quantity = 3 },
                new() { UserId = "otheruser", BookId = _testBooks[0].Id, Quantity = 2 } // Different user
            };

            await _context.CartItems.AddRangeAsync(cartItems);
            await _context.SaveChangesAsync();


            var userCartItems = await _context.CartItems
                .Where(c => c.UserId == _testUser.Id)
                .Include(c => c.Book)
                .ToListAsync();


            Assert.Equal(2, userCartItems.Count);
            Assert.All(userCartItems, item => Assert.Equal(_testUser.Id, item.UserId));
        }

        [Fact]
        public async Task UpdateCartItemQuantity_ShouldModifyQuantity()
        {

            var cartItem = new CartItem
            {
                UserId = _testUser.Id,
                BookId = _testBooks[0].Id,
                Quantity = 1
            };

            await _context.CartItems.AddAsync(cartItem);
            await _context.SaveChangesAsync();


            cartItem.Quantity = 5;
            _context.CartItems.Update(cartItem);
            await _context.SaveChangesAsync();


            var updatedCartItem = await _context.CartItems.FindAsync(cartItem.Id);
            Assert.NotNull(updatedCartItem);
            Assert.Equal(5, updatedCartItem.Quantity);
        }

        [Fact]
        public async Task RemoveFromCart_ShouldDeleteCartItem()
        {

            var cartItem = new CartItem
            {
                UserId = _testUser.Id,
                BookId = _testBooks[0].Id,
                Quantity = 2
            };

            await _context.CartItems.AddAsync(cartItem);
            await _context.SaveChangesAsync();
            var cartItemId = cartItem.Id;


            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();


            var deletedCartItem = await _context.CartItems.FindAsync(cartItemId);
            Assert.Null(deletedCartItem);
        }

        [Fact]
        public async Task ClearCart_ShouldRemoveAllUserCartItems()
        {

            var cartItems = new List<CartItem>
            {
                new() { UserId = _testUser.Id, BookId = _testBooks[0].Id, Quantity = 1 },
                new() { UserId = _testUser.Id, BookId = _testBooks[1].Id, Quantity = 2 }
            };

            await _context.CartItems.AddRangeAsync(cartItems);
            await _context.SaveChangesAsync();


            var userCartItems = await _context.CartItems
                .Where(c => c.UserId == _testUser.Id)
                .ToListAsync();

            _context.CartItems.RemoveRange(userCartItems);
            await _context.SaveChangesAsync();


            var remainingCartItems = await _context.CartItems
                .Where(c => c.UserId == _testUser.Id)
                .ToListAsync();

            Assert.Empty(remainingCartItems);
        }

        [Fact]
        public async Task GetCartTotal_ShouldCalculateCorrectTotal()
        {

            var cartItems = new List<CartItem>
            {
                new() { UserId = _testUser.Id, BookId = _testBooks[0].Id, Quantity = 2 }, // 2 * 10.99 = 21.98
                new() { UserId = _testUser.Id, BookId = _testBooks[1].Id, Quantity = 1 }  // 1 * 15.99 = 15.99
            };

            await _context.CartItems.AddRangeAsync(cartItems);
            await _context.SaveChangesAsync();


            var cartTotal = await _context.CartItems
                .Where(c => c.UserId == _testUser.Id)
                .Include(c => c.Book)
                .SumAsync(c => c.Quantity * c.Book.Price);


            Assert.Equal(37.97m, cartTotal); // 21.98 + 15.99
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
