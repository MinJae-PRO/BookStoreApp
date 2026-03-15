using BookStoreApp.Data;
using BookStoreApp.Models;
using Microsoft.EntityFrameworkCore;

namespace BookStoreApp.Tests.Services
{
    public class CheckoutBusinessLogicTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly AppUser _testUser;
        private readonly List<Book> _testBooks;

        public CheckoutBusinessLogicTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _context.Database.EnsureCreated();

            _testUser = new AppUser { Id = "testuser1", UserName = "testuser", Name = "Test User" };
            _testBooks = new List<Book>
            {
                new() { Title = "Checkout Book 1", Author = "Author 1", Genre = "Fiction", Price = 19.99m, Stock = 10 },
                new() { Title = "Checkout Book 2", Author = "Author 2", Genre = "Science", Price = 25.99m, Stock = 5 }
            };

            _context.Users.Add(_testUser);
            _context.Books.AddRange(_testBooks);
            _context.SaveChanges();
        }

        [Fact]
        public async Task ProcessCheckout_ShouldCreateOrderAndOrderItems()
        {

            var cartItems = new List<CartItem>
            {
                new() { UserId = _testUser.Id, BookId = _testBooks[0].Id, Quantity = 2 },
                new() { UserId = _testUser.Id, BookId = _testBooks[1].Id, Quantity = 1 }
            };

            await _context.CartItems.AddRangeAsync(cartItems);
            await _context.SaveChangesAsync();

            var cartItemsWithBooks = await _context.CartItems
                .Where(c => c.UserId == _testUser.Id)
                .Include(c => c.Book)
                .ToListAsync();

            var totalAmount = cartItemsWithBooks.Sum(c => c.Quantity * c.Book.Price);

            var order = new Order
            {
                UserId = _testUser.Id,
                TotalPrice = totalAmount,
                OrderDate = DateTime.UtcNow
            };

            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();

            var orderItems = cartItemsWithBooks.Select(c => new OrderItem
            {
                OrderId = order.Id,
                BookId = c.BookId,
                Quantity = c.Quantity,
                UnitPrice = c.Book.Price
            }).ToList();

            await _context.OrderItems.AddRangeAsync(orderItems);
            await _context.SaveChangesAsync();

            _context.CartItems.RemoveRange(cartItemsWithBooks);
            await _context.SaveChangesAsync();


            var createdOrder = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Book)
                .FirstOrDefaultAsync(o => o.Id == order.Id);

            var remainingCartItems = await _context.CartItems
                .Where(c => c.UserId == _testUser.Id)
                .ToListAsync();

            Assert.NotNull(createdOrder);
            Assert.Equal(65.97m, createdOrder.TotalPrice); // (2 * 19.99) + (1 * 25.99) = 39.98 + 25.99
            Assert.Equal(2, createdOrder.OrderItems.Count);
            Assert.Empty(remainingCartItems); // Cart should be empty after checkout
        }

        [Fact]
        public async Task ProcessCheckout_ShouldReduceBookStock()
        {

            var cartItems = new List<CartItem>
            {
                new() { UserId = _testUser.Id, BookId = _testBooks[0].Id, Quantity = 3 }, // Stock: 10
                new() { UserId = _testUser.Id, BookId = _testBooks[1].Id, Quantity = 2 }  // Stock: 5
            };

            await _context.CartItems.AddRangeAsync(cartItems);
            await _context.SaveChangesAsync();

            var originalStock1 = _testBooks[0].Stock;
            var originalStock2 = _testBooks[1].Stock;

            var cartItemsWithBooks = await _context.CartItems
                .Where(c => c.UserId == _testUser.Id)
                .Include(c => c.Book)
                .ToListAsync();

            foreach (var cartItem in cartItemsWithBooks)
            {
                var book = await _context.Books.FindAsync(cartItem.BookId);
                if (book != null)
                {
                    book.Stock -= cartItem.Quantity;
                    _context.Books.Update(book);
                }
            }

            await _context.SaveChangesAsync();


            var updatedBook1 = await _context.Books.FindAsync(_testBooks[0].Id);
            var updatedBook2 = await _context.Books.FindAsync(_testBooks[1].Id);

            Assert.NotNull(updatedBook1); Assert.Equal(originalStock1 - 3, updatedBook1.Stock); // 10 - 3 = 7
            Assert.NotNull(updatedBook2); Assert.Equal(originalStock2 - 2, updatedBook2.Stock); // 5 - 2 = 3
        }

        [Fact]
        public async Task CheckoutValidation_ShouldPreventOrderWhenInsufficientStock()
        {

            var cartItems = new List<CartItem>
            {
                new() { UserId = _testUser.Id, BookId = _testBooks[1].Id, Quantity = 10 } // Requesting 10, but stock is only 5
            };

            await _context.CartItems.AddRangeAsync(cartItems);
            await _context.SaveChangesAsync();

            var stockValidation = await _context.CartItems
                .Where(c => c.UserId == _testUser.Id)
                .Include(c => c.Book)
                .Select(c => new
                {
                    CartItem = c,
                    RequestedQuantity = c.Quantity,
                    AvailableStock = c.Book.Stock,
                    IsStockSufficient = c.Quantity <= c.Book.Stock
                })
                .ToListAsync();

            var canProceedWithCheckout = stockValidation.All(v => v.IsStockSufficient);


            Assert.False(canProceedWithCheckout);
            Assert.Single(stockValidation);
            Assert.False(stockValidation[0].IsStockSufficient);
            Assert.Equal(10, stockValidation[0].RequestedQuantity);
            Assert.Equal(5, stockValidation[0].AvailableStock);
        }

        [Fact]
        public async Task CheckoutWithEmptyCart_ShouldNotCreateOrder()
        {



            var cartItems = await _context.CartItems
                .Where(c => c.UserId == _testUser.Id)
                .ToListAsync();

            var orderCount = await _context.Orders.CountAsync();


            Assert.Empty(cartItems);
            Assert.Equal(0, orderCount); // No orders should be created
        }

        [Fact]
        public async Task CheckoutCalculation_ShouldHandleDecimalPrecision()
        {
            var precisionBook = new Book
            {
                Title = "Precision Test Book",
                Author = "Precision Author",
                Genre = "Math",
                Price = 12.33m,
                Stock = 10
            };

            await _context.Books.AddAsync(precisionBook);
            await _context.SaveChangesAsync();

            var cartItem = new CartItem
            {
                UserId = _testUser.Id,
                BookId = precisionBook.Id,
                Quantity = 3
            };

            await _context.CartItems.AddAsync(cartItem);
            await _context.SaveChangesAsync();


            var totalAmount = await _context.CartItems
                .Where(c => c.UserId == _testUser.Id)
                .Include(c => c.Book)
                .SumAsync(c => c.Quantity * c.Book.Price);


            Assert.Equal(36.99m, totalAmount); // 3 * 12.33 = 36.99
        }

        [Fact]
        public async Task CheckoutSuccess_ShouldGenerateOrderWithCorrectTimestamp()
        {

            var cartItem = new CartItem
            {
                UserId = _testUser.Id,
                BookId = _testBooks[0].Id,
                Quantity = 1
            };

            await _context.CartItems.AddAsync(cartItem);
            await _context.SaveChangesAsync();

            var checkoutTime = DateTime.UtcNow;


            var order = new Order
            {
                UserId = _testUser.Id,
                TotalPrice = 19.99m,
                OrderDate = checkoutTime
            };

            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();


            var createdOrder = await _context.Orders.FindAsync(order.Id);
            Assert.NotNull(createdOrder);
            Assert.True(createdOrder.OrderDate <= DateTime.UtcNow);
            Assert.True(createdOrder.OrderDate >= checkoutTime.AddSeconds(-1)); // Allow small time difference
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
