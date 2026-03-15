using System.ComponentModel.DataAnnotations;
using BookStoreApp.Models;

namespace BookStoreApp.Tests.Models
{
    public class OrderModelTests
    {
        [Fact]
        public void Order_ShouldHaveValidDefaultValues()
        {
            var order = new Order();


            Assert.Equal(0, order.Id);
            Assert.Equal(string.Empty, order.UserId);
            Assert.Equal(0, order.TotalPrice);
            Assert.NotNull(order.OrderItems);
            Assert.Empty(order.OrderItems);
            Assert.True(order.OrderDate <= DateTime.UtcNow);
        }

        [Fact]
        public void Order_ShouldSetPropertiesCorrectly()
        {

            var orderDate = DateTime.UtcNow.AddDays(-1);
            var order = new Order
            {
                Id = 1,
                UserId = "user123",
                OrderDate = orderDate,
                TotalPrice = 59.97m
            };

            Assert.Equal(1, order.Id);
            Assert.Equal("user123", order.UserId);
            Assert.Equal(orderDate, order.OrderDate);
            Assert.Equal(59.97m, order.TotalPrice);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Order_ShouldFailValidationForEmptyUserId(string userId)
        {

            var order = new Order
            {
                UserId = userId,
                TotalPrice = 100.00m
            };


            var validationResults = ValidateModel(order);


            Assert.Contains(validationResults, v => v.MemberNames.Contains("UserId"));
        }

        [Fact]
        public void Order_ShouldPassValidationWithAllValidProperties()
        {

            var order = new Order
            {
                UserId = "user123",
                TotalPrice = 99.99m
            };


            var validationResults = ValidateModel(order);


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
