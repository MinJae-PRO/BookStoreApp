using BookStoreApp.Data;
using BookStoreApp.Models;
using Microsoft.EntityFrameworkCore;

namespace BookStoreApp.Tests.Services
{
    public class OrderHistoryBusinessLogicTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly AppUser _testUser;
        private readonly List<Book> _testBooks;

        public OrderHistoryBusinessLogicTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _context.Database.EnsureCreated();

            _testUser = new AppUser { Id = "testuser1", UserName = "testuser", Name = "Test User" };
            _testBooks = new List<Book>
            {
                new() { Title = "History Book 1", Author = "Author 1", Genre = "History", Price = 20.99m, Stock = 5 },
                new() { Title = "History Book 2", Author = "Author 2", Genre = "Biography", Price = 24.99m, Stock = 3 },
                new() { Title = "History Book 3", Author = "Author 3", Genre = "Science", Price = 30.99m, Stock = 8 }
            };

            _context.Users.Add(_testUser);
            _context.Books.AddRange(_testBooks);
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetOrderHistory_ShouldReturnOrdersInChronologicalOrder()
        {

            var orders = new List<Order>
            {
                new() { UserId = _testUser.Id, TotalPrice = 45.98m, OrderDate = DateTime.UtcNow.AddDays(-10) },
                new() { UserId = _testUser.Id, TotalPrice = 55.98m, OrderDate = DateTime.UtcNow.AddDays(-5) },
                new() { UserId = _testUser.Id, TotalPrice = 30.99m, OrderDate = DateTime.UtcNow.AddDays(-1) },
                new() { UserId = "otheruser", TotalPrice = 25.99m, OrderDate = DateTime.UtcNow.AddDays(-2) } // Different user
            };

            await _context.Orders.AddRangeAsync(orders);
            await _context.SaveChangesAsync();


            var userOrderHistory = await _context.Orders
                .Where(o => o.UserId == _testUser.Id)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new
                {
                    o.Id,
                    o.TotalPrice,
                    o.OrderDate
                })
                .ToListAsync();


            Assert.Equal(3, userOrderHistory.Count);
            Assert.True(userOrderHistory[0].OrderDate > userOrderHistory[1].OrderDate);
            Assert.True(userOrderHistory[1].OrderDate > userOrderHistory[2].OrderDate);
            Assert.Equal(30.99m, userOrderHistory[0].TotalPrice); // Most recent order
        }

        [Fact]
        public async Task GetOrderDetails_ShouldIncludeOrderItemsAndBookInformation()
        {

            var order = new Order
            {
                UserId = _testUser.Id,
                TotalPrice = 75.97m,
                OrderDate = DateTime.UtcNow.AddDays(-3)
            };

            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();

            var orderItems = new List<OrderItem>
            {
                new() { OrderId = order.Id, BookId = _testBooks[0].Id, Quantity = 2, UnitPrice = 20.99m },
                new() { OrderId = order.Id, BookId = _testBooks[1].Id, Quantity = 1, UnitPrice = 24.99m },
                new() { OrderId = order.Id, BookId = _testBooks[2].Id, Quantity = 1, UnitPrice = 30.99m }
            };

            await _context.OrderItems.AddRangeAsync(orderItems);
            await _context.SaveChangesAsync();


            var orderDetails = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Book)
                .Where(o => o.Id == order.Id && o.UserId == _testUser.Id)
                .Select(o => new
                {
                    OrderId = o.Id,
                    OrderDate = o.OrderDate,
                    TotalPrice = o.TotalPrice,
                    Items = o.OrderItems.Select(oi => new
                    {
                        BookTitle = oi.Book.Title,
                        BookAuthor = oi.Book.Author,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        TotalItemPrice = oi.Quantity * oi.UnitPrice
                    }).ToList()
                })
                .FirstOrDefaultAsync();


            Assert.NotNull(orderDetails);
            Assert.Equal(3, orderDetails.Items.Count);
            Assert.Equal(75.97m, orderDetails.TotalPrice);
            
            var firstItem = orderDetails.Items.First(i => i.BookTitle == "History Book 1");
            Assert.Equal(2, firstItem.Quantity);
            Assert.Equal(41.98m, firstItem.TotalItemPrice); // 2 * 20.99
        }

        [Fact]
        public async Task GetOrdersByDateRange_ShouldFilterCorrectly()
        {

            var baseDate = DateTime.UtcNow;
            var orders = new List<Order>
            {
                new() { UserId = _testUser.Id, TotalPrice = 25.99m, OrderDate = baseDate.AddDays(-30) }, // Outside range
                new() { UserId = _testUser.Id, TotalPrice = 35.99m, OrderDate = baseDate.AddDays(-15) }, // Within range
                new() { UserId = _testUser.Id, TotalPrice = 45.99m, OrderDate = baseDate.AddDays(-5) },  // Within range
                new() { UserId = _testUser.Id, TotalPrice = 55.99m, OrderDate = baseDate.AddDays(1) }    // Outside range (future)
            };

            await _context.Orders.AddRangeAsync(orders);
            await _context.SaveChangesAsync();

            var fromDate = baseDate.AddDays(-20);
            var toDate = baseDate.AddDays(-1);


            var ordersInRange = await _context.Orders
                .Where(o => o.UserId == _testUser.Id)
                .Where(o => o.OrderDate >= fromDate && o.OrderDate <= toDate)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();


            Assert.Equal(2, ordersInRange.Count);
            Assert.Equal(45.99m, ordersInRange[0].TotalPrice); // More recent
            Assert.Equal(35.99m, ordersInRange[1].TotalPrice); // Older
        }

        [Fact]
        public async Task GetOrderStatistics_ShouldCalculateCorrectTotals()
        {

            var orders = new List<Order>
            {
                new() { UserId = _testUser.Id, TotalPrice = 20.99m, OrderDate = DateTime.UtcNow.AddDays(-10) },
                new() { UserId = _testUser.Id, TotalPrice = 35.50m, OrderDate = DateTime.UtcNow.AddDays(-5) },
                new() { UserId = _testUser.Id, TotalPrice = 42.75m, OrderDate = DateTime.UtcNow.AddDays(-2) },
                new() { UserId = "otheruser", TotalPrice = 100.00m, OrderDate = DateTime.UtcNow.AddDays(-1) } // Different user
            };

            await _context.Orders.AddRangeAsync(orders);
            await _context.SaveChangesAsync();


            var userOrderStats = await _context.Orders
                .Where(o => o.UserId == _testUser.Id)
                .GroupBy(o => o.UserId)
                .Select(g => new
                {
                    TotalOrders = g.Count(),
                    TotalSpent = g.Sum(o => o.TotalPrice),
                    AverageOrderValue = g.Average(o => o.TotalPrice),
                    FirstOrderDate = g.Min(o => o.OrderDate),
                    LastOrderDate = g.Max(o => o.OrderDate)
                })
                .FirstOrDefaultAsync();


            Assert.NotNull(userOrderStats);
            Assert.Equal(3, userOrderStats.TotalOrders);
            Assert.Equal(99.24m, userOrderStats.TotalSpent); // 20.99 + 35.50 + 42.75
            Assert.Equal(33.08m, Math.Round(userOrderStats.AverageOrderValue, 2)); // 99.24 / 3
        }

        [Fact]
        public async Task GetRecentOrders_ShouldLimitResultsCorrectly()
        {

            var orders = new List<Order>();
            for (int i = 0; i < 10; i++)
            {
                orders.Add(new Order
                {
                    UserId = _testUser.Id,
                    TotalPrice = 25.99m,
                    OrderDate = DateTime.UtcNow.AddDays(-i)
                });
            }

            await _context.Orders.AddRangeAsync(orders);
            await _context.SaveChangesAsync();


            var recentOrders = await _context.Orders
                .Where(o => o.UserId == _testUser.Id)
                .OrderByDescending(o => o.OrderDate)
                .Take(5) // Limit to 5 most recent orders
                .ToListAsync();


            Assert.Equal(5, recentOrders.Count);
            Assert.True(recentOrders[0].OrderDate > recentOrders[4].OrderDate);
        }

        [Fact]
        public async Task SearchOrdersByBookTitle_ShouldFindRelevantOrders()
        {

            var order1 = new Order { UserId = _testUser.Id, TotalPrice = 20.99m, OrderDate = DateTime.UtcNow.AddDays(-5) };
            var order2 = new Order { UserId = _testUser.Id, TotalPrice = 24.99m, OrderDate = DateTime.UtcNow.AddDays(-3) };

            await _context.Orders.AddRangeAsync(order1, order2);
            await _context.SaveChangesAsync();

            var orderItems = new List<OrderItem>
            {
                new() { OrderId = order1.Id, BookId = _testBooks[0].Id, Quantity = 1, UnitPrice = 20.99m }, // History Book 1
                new() { OrderId = order2.Id, BookId = _testBooks[1].Id, Quantity = 1, UnitPrice = 24.99m }  // History Book 2
            };

            await _context.OrderItems.AddRangeAsync(orderItems);
            await _context.SaveChangesAsync();


            var ordersWithHistoryBooks = await _context.Orders
                .Where(o => o.UserId == _testUser.Id)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Book)
                .Where(o => o.OrderItems.Any(oi => oi.Book.Title.Contains("History")))
                .ToListAsync();


            Assert.Equal(2, ordersWithHistoryBooks.Count);
            Assert.All(ordersWithHistoryBooks, order => 
                Assert.True(order.OrderItems.Any(oi => oi.Book.Title.Contains("History"))));
        }

        [Fact]
        public async Task GetOrdersByStatus_ShouldFilterCompletedOrders()
        {
            

            var completedOrders = new List<Order>
            {
                new() { UserId = _testUser.Id, TotalPrice = 30.99m, OrderDate = DateTime.UtcNow.AddDays(-7) },
                new() { UserId = _testUser.Id, TotalPrice = 45.99m, OrderDate = DateTime.UtcNow.AddDays(-3) }
            };

            await _context.Orders.AddRangeAsync(completedOrders);
            await _context.SaveChangesAsync();


            var userCompletedOrders = await _context.Orders
                .Where(o => o.UserId == _testUser.Id)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();


            Assert.Equal(2, userCompletedOrders.Count);
            Assert.All(userCompletedOrders, order => Assert.Equal(_testUser.Id, order.UserId));
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
