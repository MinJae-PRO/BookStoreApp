using BookStoreApp.Models;

namespace BookStoreApp.Tests.Utilities
{
    public static class TestDataSeeder
    {
        public static List<AppUser> CreateTestUsers()
        {
            return new List<AppUser>
            {
                new() { Id = "user1", UserName = "testuser1@example.com", Email = "testuser1@example.com", Name = "Test User One" },
                new() { Id = "user2", UserName = "testuser2@example.com", Email = "testuser2@example.com", Name = "Test User Two" },
                new() { Id = "user3", UserName = "testuser3@example.com", Email = "testuser3@example.com", Name = "Test User Three" }
            };
        }

        public static List<Book> CreateTestBooks()
        {
            return new List<Book>
            {
                new() { Title = "The Great Gatsby", Author = "F. Scott Fitzgerald", Genre = "Fiction", Price = 12.99m, Stock = 15 },
                new() { Title = "To Kill a Mockingbird", Author = "Harper Lee", Genre = "Fiction", Price = 14.99m, Stock = 10 },
                new() { Title = "1984", Author = "George Orwell", Genre = "Dystopian", Price = 13.99m, Stock = 20 },
                new() { Title = "Pride and Prejudice", Author = "Jane Austen", Genre = "Romance", Price = 11.99m, Stock = 8 },
                new() { Title = "The Catcher in the Rye", Author = "J.D. Salinger", Genre = "Fiction", Price = 15.99m, Stock = 12 },
                new() { Title = "A Brief History of Time", Author = "Stephen Hawking", Genre = "Science", Price = 18.99m, Stock = 6 },
                new() { Title = "The Art of War", Author = "Sun Tzu", Genre = "Philosophy", Price = 9.99m, Stock = 25 },
                new() { Title = "Sapiens", Author = "Yuval Noah Harari", Genre = "History", Price = 22.99m, Stock = 7 }
            };
        }

        public static List<CartItem> CreateTestCartItems(string userId, List<Book> books)
        {
            if (books.Count < 3) throw new ArgumentException("At least 3 books required");

            return new List<CartItem>
            {
                new() { UserId = userId, BookId = books[0].Id, Quantity = 2 },
                new() { UserId = userId, BookId = books[1].Id, Quantity = 1 },
                new() { UserId = userId, BookId = books[2].Id, Quantity = 3 }
            };
        }

        public static List<FavouriteBook> CreateTestFavorites(string userId, List<Book> books)
        {
            if (books.Count < 4) throw new ArgumentException("At least 4 books required");

            return new List<FavouriteBook>
            {
                new() { UserId = userId, BookId = books[0].Id },
                new() { UserId = userId, BookId = books[2].Id },
                new() { UserId = userId, BookId = books[3].Id }
            };
        }

        public static Order CreateTestOrder(string userId, decimal totalPrice, DateTime orderDate)
        {
            return new Order
            {
                UserId = userId,
                TotalPrice = totalPrice,
                OrderDate = orderDate
            };
        }

        public static List<OrderItem> CreateTestOrderItems(int orderId, List<Book> books)
        {
            if (books.Count < 2) throw new ArgumentException("At least 2 books required");

            return new List<OrderItem>
            {
                new() { OrderId = orderId, BookId = books[0].Id, Quantity = 1, UnitPrice = books[0].Price },
                new() { OrderId = orderId, BookId = books[1].Id, Quantity = 2, UnitPrice = books[1].Price }
            };
        }

        public static List<BookReview> CreateTestReviews(string userId, List<Book> books)
        {
            if (books.Count < 3) throw new ArgumentException("At least 3 books required");

            return new List<BookReview>
            {
                new() { UserId = userId, BookId = books[0].Id, Rating = 5, Comment = "Excellent book!", CreatedAt = DateTime.UtcNow.AddDays(-5) },
                new() { UserId = userId, BookId = books[1].Id, Rating = 4, Comment = "Good read.", CreatedAt = DateTime.UtcNow.AddDays(-3) },
                new() { UserId = userId, BookId = books[2].Id, Rating = 3, Comment = "Average.", CreatedAt = DateTime.UtcNow.AddDays(-1) }
            };
        }

        public static Book CreateSampleBook(string title = "Sample Book", string author = "Sample Author", 
            string genre = "Fiction", decimal price = 19.99m, int stock = 10)
        {
            return new Book
            {
                Title = title,
                Author = author,
                Genre = genre,
                Price = price,
                Stock = stock
            };
        }

        public static AppUser CreateSampleUser(string id = "testuser", string username = "testuser@example.com", 
            string name = "Test User")
        {
            return new AppUser
            {
                Id = id,
                UserName = username,
                Email = username,
                Name = name
            };
        }
    }
}