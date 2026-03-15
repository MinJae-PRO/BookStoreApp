using BookStoreApp.Data;
using BookStoreApp.Models;
using Microsoft.EntityFrameworkCore;

namespace BookStoreApp.Tests.Services
{
    public class BookCrudTests : IDisposable
    {
        private readonly AppDbContext _context;

        public BookCrudTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _context.Database.EnsureCreated();
        }

        [Fact]
        public async Task CreateBook_ShouldAddBookToDatabase()
        {

            var book = new Book
            {
                Title = "New Book",
                Author = "New Author",
                Genre = "Fiction",
                Price = 29.99m,
                Stock = 5
            };


            await _context.Books.AddAsync(book);
            await _context.SaveChangesAsync();


            var savedBook = await _context.Books.FindAsync(book.Id);
            Assert.NotNull(savedBook);
            Assert.Equal("New Book", savedBook.Title);
            Assert.Equal("New Author", savedBook.Author);
        }

        [Fact]
        public async Task GetAllBooks_ShouldReturnAllBooks()
        {

            var books = new List<Book>
            {
                new() { Title = "Book 1", Author = "Author 1", Genre = "Fiction", Price = 10.99m, Stock = 3 },
                new() { Title = "Book 2", Author = "Author 2", Genre = "Non-Fiction", Price = 15.99m, Stock = 5 },
                new() { Title = "Book 3", Author = "Author 3", Genre = "Science", Price = 20.99m, Stock = 2 }
            };

            await _context.Books.AddRangeAsync(books);
            await _context.SaveChangesAsync();


            var allBooks = await _context.Books.ToListAsync();


            Assert.Equal(3, allBooks.Count);
            Assert.Contains(allBooks, b => b.Title == "Book 1");
            Assert.Contains(allBooks, b => b.Title == "Book 2");
            Assert.Contains(allBooks, b => b.Title == "Book 3");
        }

        [Fact]
        public async Task GetBookById_ShouldReturnCorrectBook()
        {

            var book = new Book
            {
                Title = "Specific Book",
                Author = "Specific Author",
                Genre = "Mystery",
                Price = 12.99m,
                Stock = 8
            };

            await _context.Books.AddAsync(book);
            await _context.SaveChangesAsync();


            var foundBook = await _context.Books.FindAsync(book.Id);


            Assert.NotNull(foundBook);
            Assert.Equal("Specific Book", foundBook.Title);
            Assert.Equal("Specific Author", foundBook.Author);
            Assert.Equal("Mystery", foundBook.Genre);
        }

        [Fact]
        public async Task UpdateBook_ShouldModifyBookDetails()
        {

            var book = new Book
            {
                Title = "Original Title",
                Author = "Original Author",
                Genre = "Original Genre",
                Price = 19.99m,
                Stock = 10
            };

            await _context.Books.AddAsync(book);
            await _context.SaveChangesAsync();


            book.Title = "Updated Title";
            book.Author = "Updated Author";
            book.Price = 25.99m;
            book.Stock = 15;

            _context.Books.Update(book);
            await _context.SaveChangesAsync();


            var updatedBook = await _context.Books.FindAsync(book.Id);
            Assert.NotNull(updatedBook);
            Assert.Equal("Updated Title", updatedBook.Title);
            Assert.Equal("Updated Author", updatedBook.Author);
            Assert.Equal(25.99m, updatedBook.Price);
            Assert.Equal(15, updatedBook.Stock);
        }

        [Fact]
        public async Task DeleteBook_ShouldRemoveBookFromDatabase()
        {

            var book = new Book
            {
                Title = "Book to Delete",
                Author = "Delete Author",
                Genre = "Delete Genre",
                Price = 9.99m,
                Stock = 1
            };

            await _context.Books.AddAsync(book);
            await _context.SaveChangesAsync();

            var bookId = book.Id;


            _context.Books.Remove(book);
            await _context.SaveChangesAsync();


            var deletedBook = await _context.Books.FindAsync(bookId);
            Assert.Null(deletedBook);
        }

        [Fact]
        public async Task SearchBooksByTitle_ShouldReturnMatchingBooks()
        {

            var books = new List<Book>
            {
                new() { Title = "Harry Potter and the Stone", Author = "J.K. Rowling", Genre = "Fantasy", Price = 15.99m, Stock = 5 },
                new() { Title = "Harry Potter and the Chamber", Author = "J.K. Rowling", Genre = "Fantasy", Price = 16.99m, Stock = 4 },
                new() { Title = "Lord of the Rings", Author = "J.R.R. Tolkien", Genre = "Fantasy", Price = 18.99m, Stock = 3 }
            };

            await _context.Books.AddRangeAsync(books);
            await _context.SaveChangesAsync();


            var harryPotterBooks = await _context.Books
                .Where(b => b.Title.Contains("Harry Potter"))
                .ToListAsync();


            Assert.Equal(2, harryPotterBooks.Count);
            Assert.All(harryPotterBooks, book => Assert.Contains("Harry Potter", book.Title));
        }

        [Fact]
        public async Task FilterBooksByGenre_ShouldReturnCorrectBooks()
        {

            var books = new List<Book>
            {
                new() { Title = "Fiction Book 1", Author = "Author 1", Genre = "Fiction", Price = 10.99m, Stock = 2 },
                new() { Title = "Fiction Book 2", Author = "Author 2", Genre = "Fiction", Price = 11.99m, Stock = 3 },
                new() { Title = "Science Book 1", Author = "Author 3", Genre = "Science", Price = 20.99m, Stock = 1 }
            };

            await _context.Books.AddRangeAsync(books);
            await _context.SaveChangesAsync();


            var fictionBooks = await _context.Books
                .Where(b => b.Genre == "Fiction")
                .ToListAsync();


            Assert.Equal(2, fictionBooks.Count);
            Assert.All(fictionBooks, book => Assert.Equal("Fiction", book.Genre));
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
