using System.ComponentModel.DataAnnotations;
using BookStoreApp.Models;

namespace BookStoreApp.Tests.Models
{
    public class CartItemModelTests
    {
        [Fact]
        public void CartItem_ShouldHaveValidDefaultValues()
        {
            var cartItem = new CartItem();


            Assert.Equal(0, cartItem.Id);
            Assert.Equal(string.Empty, cartItem.UserId);
            Assert.Equal(0, cartItem.BookId);
            Assert.Equal(1, cartItem.Quantity);
        }

        [Fact]
        public void CartItem_ShouldSetPropertiesCorrectly()
        {

            var cartItem = new CartItem
            {
                Id = 1,
                UserId = "user123",
                BookId = 5,
                Quantity = 3
            };

            Assert.Equal(1, cartItem.Id);
            Assert.Equal("user123", cartItem.UserId);
            Assert.Equal(5, cartItem.BookId);
            Assert.Equal(3, cartItem.Quantity);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void CartItem_ShouldFailValidationForEmptyUserId(string userId)
        {

            var cartItem = new CartItem
            {
                UserId = userId,
                BookId = 1,
                Quantity = 2
            };


            var validationResults = ValidateModel(cartItem);


            Assert.Contains(validationResults, v => v.MemberNames.Contains("UserId"));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void CartItem_ShouldFailValidationForInvalidQuantity(int quantity)
        {

            var cartItem = new CartItem
            {
                UserId = "user123",
                BookId = 1,
                Quantity = quantity
            };


            var validationResults = ValidateModel(cartItem);


            Assert.Contains(validationResults, v => v.MemberNames.Contains("Quantity"));
        }

        [Fact]
        public void CartItem_ShouldPassValidationWithAllValidProperties()
        {

            var cartItem = new CartItem
            {
                UserId = "user123",
                BookId = 1,
                Quantity = 5
            };


            var validationResults = ValidateModel(cartItem);


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
