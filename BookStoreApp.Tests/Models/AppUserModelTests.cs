using System.ComponentModel.DataAnnotations;
using BookStoreApp.Models;

namespace BookStoreApp.Tests.Models
{
    public class AppUserModelTests
    {
        [Fact]
        public void AppUser_ShouldHaveValidDefaultValues()
        {
            var user = new AppUser();

            Assert.Equal(string.Empty, user.Name);
            Assert.NotNull(user.Id);
            Assert.Null(user.UserName);
            Assert.Null(user.Email);
        }

        [Fact]
        public void AppUser_ShouldSetPropertiesCorrectly()
        {
            var user = new AppUser
            {
                Id = "user123",
                UserName = "testuser@example.com",
                Email = "testuser@example.com",
                Name = "Test User"
            };

            Assert.Equal("user123", user.Id);
            Assert.Equal("testuser@example.com", user.UserName);
            Assert.Equal("testuser@example.com", user.Email);
            Assert.Equal("Test User", user.Name);
        }

        [Fact]
        public void AppUser_ShouldAllowEmptyName()
        {
            var user = new AppUser
            {
                UserName = "testuser@example.com",
                Email = "testuser@example.com",
                Name = ""
            };

            var validationResults = ValidateModel(user);

            Assert.Empty(validationResults);
        }

        [Fact]
        public void AppUser_ShouldPassValidationWithAllValidProperties()
        {
            var user = new AppUser
            {
                UserName = "testuser@example.com",
                Email = "testuser@example.com",
                Name = "Test User"
            };

            var validationResults = ValidateModel(user);

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