using BookStoreApp.Components;
using BookStoreApp.Data;
using BookStoreApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// DbContext Factory for books pages
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
        
    ));

builder.Services.AddQuickGridEntityFrameworkAdapter();

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddHttpClient();

// Identity services for user authentication and authorization
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
    {
        options.Password.RequiredLength = 6;
        options.Password.RequireDigit = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();
// Configure cookie settings for authentication
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/login";
    options.LogoutPath = "/logout";
    options.AccessDeniedPath = "/access-denied";
});
// Manage access to certain pages based on user roles
builder.Services.AddAuthorization(option =>
{
    option.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    option.AddPolicy("UserOrAdmin", policy => policy.RequireRole("User", "Admin"));
});


var app = builder.Build();
// Configure the HTTP request pipeline.
// Use different middleware based on the environment
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint(); 
}
else
{
    app.UseExceptionHandler("/Error"); 
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

// Analyse Cookie and Role 
app.UseAuthentication();
// Authorize user to access the page
app.UseAuthorization();


// validate email and sign in user, then redirect to home page and cookie creation
app.MapGet("/auth/signin", async (string email, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, HttpContext context) =>
{
    var user = await userManager.FindByEmailAsync(email);
    if (user is not null)
    {
        await signInManager.SignInAsync(user, isPersistent: false);
    }

    context.Response.Redirect("/");
});

// Sign out user and redirect to home page after delete cookie
app.MapGet("/logout", async (SignInManager<AppUser> signInManager, HttpContext context) =>
{
    await signInManager.SignOutAsync();
    context.Response.Redirect("/");
});


app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();


