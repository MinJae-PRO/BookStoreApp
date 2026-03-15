using System.ComponentModel.DataAnnotations;
using BookStoreApp.Models;

namespace BookStoreApp.Tests.Models
{
    public class OrderItemModelTests
    {
        [Fact]
        public void OrderItem_ShouldHaveValidDefaultValues()
        {
            var orderItem = new OrderItem();


            Assert.Equal(0, orderItem.Id);
            Assert.Equal(0, orderItem.OrderId);
            Assert.Equal(0, orderItem.BookId);
            Assert.Equal(0, orderItem.Quantity);
            Assert.Equal(0, orderItem.UnitPrice);
        }

        [Fact]
        public void OrderItem_ShouldSetPropertiesCorrectly()
        {

            var orderItem = new OrderItem
            {
                Id = 1,
                OrderId = 10,
                BookId = 5,
                Quantity = 3,
                UnitPrice = 19.99m
            };

            Assert.Equal(1, orderItem.Id);
            Assert.Equal(10, orderItem.OrderId);
            Assert.Equal(5, orderItem.BookId);
            Assert.Equal(3, orderItem.Quantity);
            Assert.Equal(19.99m, orderItem.UnitPrice);
        }

        [Fact]
        public void OrderItem_ShouldPassValidationForZeroBookId()
        {

            var orderItem = new OrderItem
            {
                OrderId = 1,
                BookId = 0, // Zero is technically allowed by the model
                Quantity = 2,
                UnitPrice = 15.00m
            };


            var validationResults = ValidateModel(orderItem);

            Assert.Empty(validationResults);
        }

        [Fact]
        public void OrderItem_ShouldPassValidationForZeroQuantity()
        {

            var orderItem = new OrderItem
            {
                OrderId = 1,
                BookId = 1,
                Quantity = 0, // Zero is allowed by the model
                UnitPrice = 15.00m
            };


            var validationResults = ValidateModel(orderItem);

            Assert.Empty(validationResults);
        }

        [Fact]
        public void OrderItem_ShouldPassValidationForZeroUnitPrice()
        {

            var orderItem = new OrderItem
            {
                OrderId = 1,
                BookId = 1,
                Quantity = 2,
                UnitPrice = 0 // Zero is allowed by the model
            };


            var validationResults = ValidateModel(orderItem);

            Assert.Empty(validationResults);
        }

        [Fact]
        public void OrderItem_ShouldPassValidationWithAllValidProperties()
        {

            var orderItem = new OrderItem
            {
                OrderId = 1,
                BookId = 1,
                Quantity = 2,
                UnitPrice = 19.99m
            };


            var validationResults = ValidateModel(orderItem);


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
