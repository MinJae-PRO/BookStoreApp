using System.ComponentModel.DataAnnotations;
using BookStoreApp.Models;

namespace BookStoreApp.Tests.Models
{
    public class BookModelTests
    {
        [Fact]
        public void Book_ShouldHaveValidDefaultValues()
        {
            var book = new Book();


            Assert.Equal(0, book.Id);
            Assert.Equal(string.Empty, book.Title);
            Assert.Equal(string.Empty, book.Author);
            Assert.Equal(string.Empty, book.Genre);
            Assert.Equal(0, book.Price);
            Assert.Equal(0, book.Stock);
        }

        [Fact]
        public void Book_ShouldSetPropertiesCorrectly()
        {

            var book = new Book
            {
                Id = 1,
                Title = "Test Book",
                Author = "Test Author",
                Genre = "Fiction",
                Price = 19.99m,
                Stock = 10
            };

            Assert.Equal(1, book.Id);
            Assert.Equal("Test Book", book.Title);
            Assert.Equal("Test Author", book.Author);
            Assert.Equal("Fiction", book.Genre);
            Assert.Equal(19.99m, book.Price);
            Assert.Equal(10, book.Stock);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Book_ShouldFailValidationForEmptyTitle(string title)
        {

            var book = new Book
            {
                Title = title,
                Author = "Test Author",
                Genre = "Fiction",
                Price = 19.99m,
                Stock = 10
            };


            var validationResults = ValidateModel(book);


            Assert.Contains(validationResults, v => v.ErrorMessage == "Title is required.");
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Book_ShouldFailValidationForEmptyAuthor(string author)
        {

            var book = new Book
            {
                Title = "Test Book",
                Author = author,
                Genre = "Fiction",
                Price = 19.99m,
                Stock = 10
            };


            var validationResults = ValidateModel(book);


            Assert.Contains(validationResults, v => v.ErrorMessage == "Author name is required.");
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Book_ShouldFailValidationForEmptyGenre(string genre)
        {

            var book = new Book
            {
                Title = "Test Book",
                Author = "Test Author",
                Genre = genre,
                Price = 19.99m,
                Stock = 10
            };


            var validationResults = ValidateModel(book);


            Assert.Contains(validationResults, v => v.ErrorMessage == "Genre is required.");
        }

        [Fact]
        public void Book_ShouldPassValidationWithAllValidProperties()
        {

            var book = new Book
            {
                Title = "Valid Book",
                Author = "Valid Author",
                Genre = "Fiction",
                Price = 19.99m,
                Stock = 10
            };


            var validationResults = ValidateModel(book);


            Assert.Empty(validationResults);
        }

        private static List<ValidationResult> ValidateModel(object model)
        {
            var context = new ValidationContext(model);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(model, context, results, true);
            return results;
        }
    }
}
