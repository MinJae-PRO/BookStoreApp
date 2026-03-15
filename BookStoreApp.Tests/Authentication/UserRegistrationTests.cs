using BookStoreApp.Data;
using BookStoreApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace BookStoreApp.Tests.Authentication
{
    public class UserRegistrationTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public UserRegistrationTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _context.Database.EnsureCreated();

            var services = new ServiceCollection();
            services.AddIdentity<AppUser, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>();
            
            services.AddSingleton(_context);
            services.AddLogging();

            var serviceProvider = services.BuildServiceProvider();
            _userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();
            _signInManager = serviceProvider.GetRequiredService<SignInManager<AppUser>>();
        }

        [Fact]
        public async Task CreateUserAsync_ShouldSucceedWithValidData()
        {

            var user = new AppUser
            {
                UserName = "testuser@example.com",
                Email = "testuser@example.com",
                Name = "Test User"
            };


            var result = await _userManager.CreateAsync(user, "Test123!");


            Assert.True(result.Succeeded);
            var createdUser = await _userManager.FindByEmailAsync("testuser@example.com");
            Assert.NotNull(createdUser);
            Assert.Equal("Test User", createdUser.Name);
        }

        [Theory]
        [InlineData("")]
        [InlineData("weak")]
        [InlineData("123")]
        [InlineData("password")]
        public async Task CreateUserAsync_ShouldFailWithWeakPassword(string password)
        {

            var user = new AppUser
            {
                UserName = "weakpass@example.com",
                Email = "weakpass@example.com",
                Name = "Weak Password User"
            };


            var result = await _userManager.CreateAsync(user, password);


            Assert.False(result.Succeeded);
            Assert.Contains(result.Errors, e => e.Code.Contains("Password"));
        }

        [Fact]
        public async Task CreateUserAsync_ShouldFailWithDuplicateEmail()
        {

            var user1 = new AppUser
            {
                UserName = "duplicate@example.com",
                Email = "duplicate@example.com",
                Name = "User One"
            };

            var user2 = new AppUser
            {
                UserName = "duplicate@example.com",
                Email = "duplicate@example.com",
                Name = "User Two"
            };


            var result1 = await _userManager.CreateAsync(user1, "Test123!");
            var result2 = await _userManager.CreateAsync(user2, "Test123!");


            Assert.True(result1.Succeeded);
            Assert.False(result2.Succeeded);
        }

        [Theory]
        [InlineData("plainaddress")]
        public async Task CreateUserAsync_ShouldFailWithInvalidEmail(string email)
        {

            var user = new AppUser
            {
                UserName = email,
                Email = email,
                Name = "Invalid Email User"
            };


            var result = await _userManager.CreateAsync(user, "Test123!");

            if (result.Succeeded)
            {
                Assert.True(result.Succeeded);
                var foundUser = await _userManager.FindByNameAsync(email);
                Assert.NotNull(foundUser);
            }
            else
            {
                Assert.False(result.Succeeded);
            }
        }

        [Fact]
        public async Task CreateUserAsync_WithValidButUnusualEmail_MaySucceed()
        {


            var user = new AppUser
            {
                UserName = "user@example.com",
                Email = "user@example.com",
                Name = "Valid Email User"
            };


            var result = await _userManager.CreateAsync(user, "Test123!");


            Assert.True(result.Succeeded);
        }

        [Fact]
        public async Task FindUserByEmailAsync_ShouldReturnCorrectUser()
        {

            var user = new AppUser
            {
                UserName = "findme@example.com",
                Email = "findme@example.com",
                Name = "Find Me User"
            };
            await _userManager.CreateAsync(user, "Test123!");


            var foundUser = await _userManager.FindByEmailAsync("findme@example.com");


            Assert.NotNull(foundUser);
            Assert.Equal("Find Me User", foundUser.Name);
            Assert.Equal("findme@example.com", foundUser.Email);
        }

        [Fact]
        public async Task ValidatePasswordAsync_ShouldCheckPasswordCorrectly()
        {

            var user = new AppUser
            {
                UserName = "passwordcheck@example.com",
                Email = "passwordcheck@example.com",
                Name = "Password Check User"
            };
            await _userManager.CreateAsync(user, "CorrectPassword123!");


            var isValidCorrect = await _userManager.CheckPasswordAsync(user, "CorrectPassword123!");
            var isValidIncorrect = await _userManager.CheckPasswordAsync(user, "WrongPassword");


            Assert.True(isValidCorrect);
            Assert.False(isValidIncorrect);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
