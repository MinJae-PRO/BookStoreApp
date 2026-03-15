using BookStoreApp.Data;
using BookStoreApp.Models;
using Microsoft.EntityFrameworkCore;

namespace BookStoreApp.Tests.Utilities
{
    public class TestDbContextFactory
    {
        public static AppDbContext CreateInMemoryContext(string? databaseName = null)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: databaseName ?? Guid.NewGuid().ToString())
                .Options;

            var context = new AppDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        public static AppDbContext CreateContextWithData(string? databaseName = null)
        {
            var context = CreateInMemoryContext(databaseName);
            SeedTestData(context);
            return context;
        }

        private static void SeedTestData(AppDbContext context)
        {
            var testUsers = TestDataSeeder.CreateTestUsers();
            context.Users.AddRange(testUsers);

            var testBooks = TestDataSeeder.CreateTestBooks();
            context.Books.AddRange(testBooks);

            context.SaveChanges();
        }
    }
}
