using BookStoreApp.Data;
using BookStoreApp.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Security.Claims;

namespace BookStoreApp.Tests.Authentication
{
    public class UserLoginTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public UserLoginTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _context.Database.EnsureCreated();

            var services = new ServiceCollection();
            services.AddDbContext<AppDbContext>(opt => opt.UseInMemoryDatabase(Guid.NewGuid().ToString()));
            services.AddIdentity<AppUser, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>();

            services.AddLogging();

            var serviceProvider = services.BuildServiceProvider();
            _userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();
        }

        [Fact]
        public async Task CreateUserAndValidatePassword_ShouldWork()
        {
            var user = new AppUser
            {
                UserName = "logintest@example.com",
                Email = "logintest@example.com",
                Name = "Login Test User"
            };

            var createResult = await _userManager.CreateAsync(user, "LoginTest123!");
            var passwordValid = await _userManager.CheckPasswordAsync(user, "LoginTest123!");
            var passwordInvalid = await _userManager.CheckPasswordAsync(user, "WrongPassword123!");

            Assert.True(createResult.Succeeded);
            Assert.True(passwordValid);
            Assert.False(passwordInvalid);
        }

        [Fact]
        public async Task FindUserByEmailAndValidatePassword_ShouldWork()
        {
            var user = new AppUser
            {
                UserName = "findtest@example.com",
                Email = "findtest@example.com",
                Name = "Find Test User"
            };
            await _userManager.CreateAsync(user, "FindTest123!");

            var foundUser = await _userManager.FindByEmailAsync("findtest@example.com");

            Assert.NotNull(foundUser);
            Assert.Equal("Find Test User", foundUser.Name);

            var passwordCheck = await _userManager.CheckPasswordAsync(foundUser, "FindTest123!");
            Assert.True(passwordCheck);
        }

        [Fact]
        public async Task CheckPasswordAsync_ShouldValidatePasswordCorrectly()
        {
            var user = new AppUser
            {
                UserName = "passcheck@example.com",
                Email = "passcheck@example.com",
                Name = "Password Check User"
            };
            await _userManager.CreateAsync(user, "CheckPassword123!");

            var isCorrect = await _userManager.CheckPasswordAsync(user, "CheckPassword123!");
            var isIncorrect = await _userManager.CheckPasswordAsync(user, "WrongPassword123!");

            Assert.True(isCorrect);
            Assert.False(isIncorrect);
        }

        [Theory]
        [InlineData("loginuser@example.com")]
        [InlineData("LOGINUSER@EXAMPLE.COM")]
        [InlineData("LoginUser@Example.Com")]
        public async Task FindUserByEmailAsync_ShouldBeCaseInsensitive(string emailToFind)
        {
            var user = new AppUser
            {
                UserName = "loginuser@example.com",
                Email = "loginuser@example.com",
                Name = "Case Test User"
            };
            await _userManager.CreateAsync(user, "CaseTest123!");

            var foundUser = await _userManager.FindByEmailAsync(emailToFind);

            Assert.NotNull(foundUser);
            Assert.Equal("Case Test User", foundUser.Name);
        }

        [Fact]
        public async Task UpdateSecurityStampAsync_ShouldInvalidateExistingSessions()
        {
            var user = new AppUser
            {
                UserName = "securitytest@example.com",
                Email = "securitytest@example.com",
                Name = "Security Test User"
            };
            await _userManager.CreateAsync(user, "SecurityTest123!");

            var originalStamp = user.SecurityStamp;

            await _userManager.UpdateSecurityStampAsync(user);

            var updatedUser = await _userManager.FindByEmailAsync("securitytest@example.com");
            Assert.NotEqual(originalStamp, updatedUser!.SecurityStamp);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}