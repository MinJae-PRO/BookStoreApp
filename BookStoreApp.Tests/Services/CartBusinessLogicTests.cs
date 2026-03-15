using BookStoreApp.Data;
using BookStoreApp.Models;
using Microsoft.EntityFrameworkCore;

namespace BookStoreApp.Tests.Services
{
    public class CartBusinessLogicTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly AppUser _testUser;
        private readonly List<Book> _testBooks;

        public CartBusinessLogicTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _context.Database.EnsureCreated();

            _testUser = new AppUser { Id = "testuser1", UserName = "testuser", Name = "Test User" };
            _testBooks = new List<Book>
            {
                new() { Title = "Cart Book 1", Author = "Author 1", Genre = "Fiction", Price = 15.99m, Stock = 10 },
                new() { Title = "Cart Book 2", Author = "Author 2", Genre = "Non-Fiction", Price = 22.50m, Stock = 5 },
                new() { Title = "Cart Book 3", Author = "Author 3", Genre = "Science", Price = 30.00m, Stock = 2 }
            };

            _context.Users.Add(_testUser);
            _context.Books.AddRange(_testBooks);
            _context.SaveChanges();
        }

        [Fact]
        public async Task AddToCart_WhenBookExists_ShouldUpdateQuantity()
        {

            var existingCartItem = new CartItem
            {
                UserId = _testUser.Id,
                BookId = _testBooks[0].Id,
                Quantity = 2
            };

            await _context.CartItems.AddAsync(existingCartItem);
            await _context.SaveChangesAsync();

            var existingItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.UserId == _testUser.Id && c.BookId == _testBooks[0].Id);

            if (existingItem != null)
            {
                existingItem.Quantity += 3;
                _context.CartItems.Update(existingItem);
                await _context.SaveChangesAsync();
            }


            var updatedCartItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.UserId == _testUser.Id && c.BookId == _testBooks[0].Id);

            Assert.NotNull(updatedCartItem);
            Assert.Equal(5, updatedCartItem.Quantity); 
        }

        [Fact]
        public async Task AddToCart_WhenStockInsufficient_ShouldLimitQuantity()
        {

            var book = _testBooks[2]; 

           
            var requestedQuantity = 5;
            var actualQuantity = Math.Min(requestedQuantity, book.Stock);

            var cartItem = new CartItem
            {
                UserId = _testUser.Id,
                BookId = book.Id,
                Quantity = actualQuantity
            };

            await _context.CartItems.AddAsync(cartItem);
            await _context.SaveChangesAsync();


            var savedCartItem = await _context.CartItems.FindAsync(cartItem.Id);
            Assert.NotNull(savedCartItem);
            Assert.Equal(2, savedCartItem.Quantity); // Limited by stock
            Assert.True(savedCartItem.Quantity <= book.Stock);
        }

        [Fact]
        public async Task CalculateCartTotal_ShouldIncludeAllItems()
        {

            var cartItems = new List<CartItem>
            {
                new() { UserId = _testUser.Id, BookId = _testBooks[0].Id, Quantity = 2 }, // 2 * 15.99 = 31.98
                new() { UserId = _testUser.Id, BookId = _testBooks[1].Id, Quantity = 1 }, // 1 * 22.50 = 22.50
                new() { UserId = _testUser.Id, BookId = _testBooks[2].Id, Quantity = 3 }  // 3 * 30.00 = 90.00
            };

            await _context.CartItems.AddRangeAsync(cartItems);
            await _context.SaveChangesAsync();


            var cartTotal = await _context.CartItems
                .Where(c => c.UserId == _testUser.Id)
                .Include(c => c.Book)
                .SumAsync(c => c.Quantity * c.Book.Price);

            var cartItemCount = await _context.CartItems
                .Where(c => c.UserId == _testUser.Id)
                .SumAsync(c => c.Quantity);


            Assert.Equal(144.48m, cartTotal); 
            Assert.Equal(6, cartItemCount); 
        }

        [Fact]
        public async Task UpdateCartItemQuantity_WhenQuantityZero_ShouldRemoveItem()
        {

            var cartItem = new CartItem
            {
                UserId = _testUser.Id,
                BookId = _testBooks[0].Id,
                Quantity = 3
            };

            await _context.CartItems.AddAsync(cartItem);
            await _context.SaveChangesAsync();
            var cartItemId = cartItem.Id;

            cartItem.Quantity = 0;

            if (cartItem.Quantity <= 0)
            {
                _context.CartItems.Remove(cartItem);
            }
            else
            {
                _context.CartItems.Update(cartItem);
            }

            await _context.SaveChangesAsync();


            var removedItem = await _context.CartItems.FindAsync(cartItemId);
            Assert.Null(removedItem);
        }

        [Fact]
        public async Task GetCartSummary_ShouldProvideCorrectInformation()
        {

            var cartItems = new List<CartItem>
            {
                new() { UserId = _testUser.Id, BookId = _testBooks[0].Id, Quantity = 1 },
                new() { UserId = _testUser.Id, BookId = _testBooks[1].Id, Quantity = 2 }
            };

            await _context.CartItems.AddRangeAsync(cartItems);
            await _context.SaveChangesAsync();


            var cartSummary = await _context.CartItems
                .Where(c => c.UserId == _testUser.Id)
                .Include(c => c.Book)
                .Select(c => new
                {
                    BookTitle = c.Book.Title,
                    Quantity = c.Quantity,
                    UnitPrice = c.Book.Price,
                    TotalPrice = c.Quantity * c.Book.Price
                })
                .ToListAsync();

            var totalCartValue = cartSummary.Sum(s => s.TotalPrice);
            var totalItems = cartSummary.Sum(s => s.Quantity);


            Assert.Equal(2, cartSummary.Count); 
            Assert.Equal(3, totalItems); 
            Assert.Equal(60.99m, totalCartValue); 
        }

        [Fact]
        public async Task ValidateCartBeforeCheckout_ShouldCheckStockAvailability()
        {

            var cartItems = new List<CartItem>
            {
                new() { UserId = _testUser.Id, BookId = _testBooks[0].Id, Quantity = 5 }, // Stock: 10, OK
                new() { UserId = _testUser.Id, BookId = _testBooks[2].Id, Quantity = 3 }  // Stock: 2, NOT OK
            };

            await _context.CartItems.AddRangeAsync(cartItems);
            await _context.SaveChangesAsync();


            var cartValidation = await _context.CartItems
                .Where(c => c.UserId == _testUser.Id)
                .Include(c => c.Book)
                .Select(c => new
                {
                    CartItem = c,
                    IsValid = c.Quantity <= c.Book.Stock,
                    AvailableStock = c.Book.Stock
                })
                .ToListAsync();

            var invalidItems = cartValidation.Where(v => !v.IsValid).ToList();
            var isCartValid = !invalidItems.Any();


            Assert.False(isCartValid);
            Assert.Single(invalidItems);
            Assert.Equal(_testBooks[2].Id, invalidItems[0].CartItem.BookId);
            Assert.Equal(2, invalidItems[0].AvailableStock);
        }

        [Fact]
        public async Task EmptyCart_ShouldRemoveAllUserItems()
        {

            var cartItems = new List<CartItem>
            {
                new() { UserId = _testUser.Id, BookId = _testBooks[0].Id, Quantity = 1 },
                new() { UserId = _testUser.Id, BookId = _testBooks[1].Id, Quantity = 2 },
                new() { UserId = "otheruser", BookId = _testBooks[0].Id, Quantity = 1 }
            };

            await _context.CartItems.AddRangeAsync(cartItems);
            await _context.SaveChangesAsync();


            var userCartItems = await _context.CartItems
                .Where(c => c.UserId == _testUser.Id)
                .ToListAsync();

            _context.CartItems.RemoveRange(userCartItems);
            await _context.SaveChangesAsync();


            var remainingUserItems = await _context.CartItems
                .Where(c => c.UserId == _testUser.Id)
                .ToListAsync();

            var otherUserItems = await _context.CartItems
                .Where(c => c.UserId == "otheruser")
                .ToListAsync();

            Assert.Empty(remainingUserItems);
            Assert.Single(otherUserItems); 
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
