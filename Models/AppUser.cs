using Microsoft.AspNetCore.Identity;

// Implements IdentityUser
namespace BookStoreApp.Models
{
    public class AppUser : IdentityUser
    {
        public string Name { get; set; } = "";
    }
}
