using Microsoft.EntityFrameworkCore;
using Xunit;
using BookStoreApp.Data;
using BookStoreApp.Models;
using BookStoreApp.Tests.Utilities;

namespace BookStoreApp.Tests.Data;

public class AppDbContextTests : DatabaseTestBase
{
    [Fact]
    public async Task CanAddAndRetrieveBook()
    {

        var book = TestDataSeeder.CreateTestBooks().First();


        Context.Books.Add(book);
        await Context.SaveChangesAsync();


        var savedBook = await Context.Books.FirstOrDefaultAsync(b => b.Title == book.Title);
        Assert.NotNull(savedBook);
        Assert.Equal(book.Title, savedBook.Title);
        Assert.Equal(book.Author, savedBook.Author);
    }

    [Fact]
    public async Task CanAddAndRetrieveUser()
    {

        var user = TestDataSeeder.CreateTestUsers().First();


        Context.Users.Add(user);
        await Context.SaveChangesAsync();


        var savedUser = await Context.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
        Assert.NotNull(savedUser);
        Assert.Equal(user.Email, savedUser.Email);
        Assert.Equal(user.UserName, savedUser.UserName);
    }

    [Fact]
    public async Task CanCreateOrderWithItems()
    {

        var user = TestDataSeeder.CreateTestUsers().First();
        var book = TestDataSeeder.CreateTestBooks().First();

        Context.Users.Add(user);
        Context.Books.Add(book);
        await Context.SaveChangesAsync();

        var order = new Order
        {
            UserId = user.Id,
            OrderDate = DateTime.UtcNow,
            TotalPrice = 19.99m
        };

        var orderItem = new OrderItem
        {
            Order = order,
            BookId = book.Id,
            Quantity = 1,
            UnitPrice = 19.99m
        };

        order.OrderItems = new List<OrderItem> { orderItem };


        Context.Orders.Add(order);
        await Context.SaveChangesAsync();


        var savedOrder = await Context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.UserId == user.Id);

        Assert.NotNull(savedOrder);
        Assert.Single(savedOrder.OrderItems);
        Assert.Equal(19.99m, savedOrder.TotalPrice);
    }
}
