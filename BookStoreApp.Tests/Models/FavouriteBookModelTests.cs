using System.ComponentModel.DataAnnotations;
using BookStoreApp.Models;

namespace BookStoreApp.Tests.Models
{
    public class FavouriteBookModelTests
    {
        [Fact]
        public void FavouriteBook_ShouldHaveValidDefaultValues()
        {
            var favouriteBook = new FavouriteBook();


            Assert.Equal(0, favouriteBook.Id);
            Assert.Equal(string.Empty, favouriteBook.UserId);
            Assert.Equal(0, favouriteBook.BookId);
        }

        [Fact]
        public void FavouriteBook_ShouldSetPropertiesCorrectly()
        {

            var favouriteBook = new FavouriteBook
            {
                Id = 1,
                UserId = "user123",
                BookId = 5
            };

            Assert.Equal(1, favouriteBook.Id);
            Assert.Equal("user123", favouriteBook.UserId);
            Assert.Equal(5, favouriteBook.BookId);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void FavouriteBook_ShouldFailValidationForEmptyUserId(string userId)
        {

            var favouriteBook = new FavouriteBook
            {
                UserId = userId,
                BookId = 1
            };


            var validationResults = ValidateModel(favouriteBook);


            Assert.Contains(validationResults, v => v.MemberNames.Contains("UserId"));
        }

        [Fact]
        public void FavouriteBook_ShouldPassValidationWithAllValidProperties()
        {

            var favouriteBook = new FavouriteBook
            {
                UserId = "user123",
                BookId = 1
            };


            var validationResults = ValidateModel(favouriteBook);


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
