using BookStoreApp.Data;
using BookStoreApp.Models;
using Microsoft.EntityFrameworkCore;

namespace BookStoreApp.Tests.Services
{
    public class OrderCrudTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly AppUser _testUser;
        private readonly List<Book> _testBooks;

        public OrderCrudTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _context.Database.EnsureCreated();

            _testUser = new AppUser { Id = "testuser1", UserName = "testuser", Name = "Test User" };
            _testBooks = new List<Book>
            {
                new() { Title = "Order Book 1", Author = "Author 1", Genre = "Fiction", Price = 19.99m, Stock = 10 },
                new() { Title = "Order Book 2", Author = "Author 2", Genre = "Non-Fiction", Price = 25.99m, Stock = 5 }
            };

            _context.Users.Add(_testUser);
            _context.Books.AddRange(_testBooks);
            _context.SaveChanges();
        }

        [Fact]
        public async Task CreateOrder_ShouldAddOrderToDatabase()
        {

            var order = new Order
            {
                UserId = _testUser.Id,
                TotalPrice = 45.98m,
                OrderDate = DateTime.UtcNow
            };


            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();


            var savedOrder = await _context.Orders.FindAsync(order.Id);
            Assert.NotNull(savedOrder);
            Assert.Equal(_testUser.Id, savedOrder.UserId);
            Assert.Equal(45.98m, savedOrder.TotalPrice);
        }

        [Fact]
        public async Task CreateOrderWithItems_ShouldAddOrderAndOrderItems()
        {

            var order = new Order
            {
                UserId = _testUser.Id,
                TotalPrice = 65.97m,
                OrderDate = DateTime.UtcNow
            };

            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();

            var orderItems = new List<OrderItem>
            {
                new() { OrderId = order.Id, BookId = _testBooks[0].Id, Quantity = 2, UnitPrice = 19.99m },
                new() { OrderId = order.Id, BookId = _testBooks[1].Id, Quantity = 1, UnitPrice = 25.99m }
            };


            await _context.OrderItems.AddRangeAsync(orderItems);
            await _context.SaveChangesAsync();


            var savedOrder = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Book)
                .FirstOrDefaultAsync(o => o.Id == order.Id);

            Assert.NotNull(savedOrder);
            Assert.Equal(2, savedOrder.OrderItems.Count);
            Assert.Equal(65.97m, savedOrder.TotalPrice);
        }

        [Fact]
        public async Task GetUserOrders_ShouldReturnUserOrdersOnly()
        {

            var userOrder = new Order
            {
                UserId = _testUser.Id,
                TotalPrice = 19.99m,
                OrderDate = DateTime.UtcNow.AddDays(-1)
            };

            var otherUserOrder = new Order
            {
                UserId = "otheruser",
                TotalPrice = 29.99m,
                OrderDate = DateTime.UtcNow.AddDays(-2)
            };

            await _context.Orders.AddRangeAsync(userOrder, otherUserOrder);
            await _context.SaveChangesAsync();


            var userOrders = await _context.Orders
                .Where(o => o.UserId == _testUser.Id)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();


            Assert.Single(userOrders);
            Assert.Equal(_testUser.Id, userOrders[0].UserId);
        }

        [Fact]
        public async Task GetOrderById_ShouldReturnOrderWithDetails()
        {

            var order = new Order
            {
                UserId = _testUser.Id,
                TotalPrice = 45.98m,
                OrderDate = DateTime.UtcNow
            };

            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();

            var orderItems = new List<OrderItem>
            {
                new() { OrderId = order.Id, BookId = _testBooks[0].Id, Quantity = 1, UnitPrice = 19.99m },
                new() { OrderId = order.Id, BookId = _testBooks[1].Id, Quantity = 1, UnitPrice = 25.99m }
            };

            await _context.OrderItems.AddRangeAsync(orderItems);
            await _context.SaveChangesAsync();


            var retrievedOrder = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Book)
                .FirstOrDefaultAsync(o => o.Id == order.Id);


            Assert.NotNull(retrievedOrder);
            Assert.Equal(2, retrievedOrder.OrderItems.Count);
            Assert.Contains(retrievedOrder.OrderItems, oi => oi.Book.Title == "Order Book 1");
            Assert.Contains(retrievedOrder.OrderItems, oi => oi.Book.Title == "Order Book 2");
        }

        [Fact]
        public async Task CalculateOrderTotal_ShouldSumOrderItemsCorrectly()
        {

            var order = new Order
            {
                UserId = _testUser.Id,
                TotalPrice = 0, // Will be calculated
                OrderDate = DateTime.UtcNow
            };

            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();

            var orderItems = new List<OrderItem>
            {
                new() { OrderId = order.Id, BookId = _testBooks[0].Id, Quantity = 3, UnitPrice = 19.99m }, // 59.97
                new() { OrderId = order.Id, BookId = _testBooks[1].Id, Quantity = 2, UnitPrice = 25.99m }  // 51.98
            };

            await _context.OrderItems.AddRangeAsync(orderItems);
            await _context.SaveChangesAsync();


            var calculatedTotal = await _context.OrderItems
                .Where(oi => oi.OrderId == order.Id)
                .SumAsync(oi => oi.Quantity * oi.UnitPrice);

            order.TotalPrice = calculatedTotal;
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();


            var updatedOrder = await _context.Orders.FindAsync(order.Id);
            Assert.NotNull(updatedOrder); Assert.Equal(111.95m, updatedOrder.TotalPrice); // 59.97 + 51.98
        }

        [Fact]
        public async Task GetOrderHistory_ShouldReturnOrdersInChronologicalOrder()
        {

            var orders = new List<Order>
            {
                new() { UserId = _testUser.Id, TotalPrice = 19.99m, OrderDate = DateTime.UtcNow.AddDays(-5) },
                new() { UserId = _testUser.Id, TotalPrice = 29.99m, OrderDate = DateTime.UtcNow.AddDays(-2) },
                new() { UserId = _testUser.Id, TotalPrice = 39.99m, OrderDate = DateTime.UtcNow.AddDays(-1) }
            };

            await _context.Orders.AddRangeAsync(orders);
            await _context.SaveChangesAsync();


            var orderHistory = await _context.Orders
                .Where(o => o.UserId == _testUser.Id)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();


            Assert.Equal(3, orderHistory.Count);
            Assert.True(orderHistory[0].OrderDate > orderHistory[1].OrderDate);
            Assert.True(orderHistory[1].OrderDate > orderHistory[2].OrderDate);
            Assert.Equal(39.99m, orderHistory[0].TotalPrice); // Most recent order
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
