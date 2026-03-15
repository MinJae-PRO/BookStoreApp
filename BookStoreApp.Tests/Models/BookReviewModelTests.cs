using System.ComponentModel.DataAnnotations;
using BookStoreApp.Models;

namespace BookStoreApp.Tests.Models
{
    public class BookReviewModelTests
    {
        [Fact]
        public void BookReview_ShouldHaveValidDefaultValues()
        {
            var review = new BookReview();


            Assert.Equal(0, review.Id);
            Assert.Equal(0, review.BookId);
            Assert.Equal(string.Empty, review.UserId);
            Assert.Equal(0, review.Rating);
            Assert.Equal(string.Empty, review.Comment);
            Assert.True(review.CreatedAt <= DateTime.UtcNow);
        }

        [Fact]
        public void BookReview_ShouldSetPropertiesCorrectly()
        {

            var createdDate = DateTime.UtcNow.AddDays(-1);
            var review = new BookReview
            {
                Id = 1,
                BookId = 5,
                UserId = "user123",
                Rating = 4,
                Comment = "Great book!",
                CreatedAt = createdDate
            };

            Assert.Equal(1, review.Id);
            Assert.Equal(5, review.BookId);
            Assert.Equal("user123", review.UserId);
            Assert.Equal(4, review.Rating);
            Assert.Equal("Great book!", review.Comment);
            Assert.Equal(createdDate, review.CreatedAt);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void BookReview_ShouldFailValidationForEmptyUserId(string userId)
        {

            var review = new BookReview
            {
                BookId = 1,
                UserId = userId,
                Rating = 4,
                Comment = "Good book!"
            };


            var validationResults = ValidateModel(review);


            Assert.Contains(validationResults, v => v.MemberNames.Contains("UserId"));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(6)]
        [InlineData(-1)]
        public void BookReview_ShouldFailValidationForInvalidRating(int rating)
        {

            var review = new BookReview
            {
                BookId = 1,
                UserId = "user123",
                Rating = rating,
                Comment = "Test comment"
            };


            var validationResults = ValidateModel(review);


            Assert.Contains(validationResults, v => v.ErrorMessage == "Rating must be between 1 and 5.");
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void BookReview_ShouldFailValidationForEmptyComment(string comment)
        {

            var review = new BookReview
            {
                BookId = 1,
                UserId = "user123",
                Rating = 4,
                Comment = comment
            };


            var validationResults = ValidateModel(review);


            Assert.Contains(validationResults, v => v.ErrorMessage == "Comment is required.");
        }

        [Fact]
        public void BookReview_ShouldFailValidationForTooLongComment()
        {

            var longComment = new string('a', 1001); // 1001 characters, exceeds max length of 1000
            var review = new BookReview
            {
                BookId = 1,
                UserId = "user123",
                Rating = 4,
                Comment = longComment
            };


            var validationResults = ValidateModel(review);


            Assert.Contains(validationResults, v => v.MemberNames.Contains("Comment"));
        }

        [Fact]
        public void BookReview_ShouldPassValidationWithAllValidProperties()
        {

            var review = new BookReview
            {
                BookId = 1,
                UserId = "user123",
                Rating = 5,
                Comment = "Excellent book, highly recommended!"
            };


            var validationResults = ValidateModel(review);


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
