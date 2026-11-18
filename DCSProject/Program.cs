using DCSProject.Controllers;
using DCSProject.Data;
using DCSProject.Models;
using DCSProject.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Lets the site remember data for a while (used to cache the Steam response)
builder.Services.AddMemoryCache();

// Lets SteamService receive an HttpClient so it can call the Steam API
// Timeout added to ensure if steam is slow it does not hang up the page
builder.Services.AddHttpClient<SteamService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
});

// Connection string comes from User Secrets (never from a committed file). See docs/DATABASE.md
string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found. Set it with User Secrets - see docs/DATABASE.md.");

// Configure DB Context settings
builder.Services.AddDbContext<DcsDb>(options => options.UseSqlServer(connectionString));

// Login system (ASP.NET Identity). Handles users, hashed passwords, roles and the login cookie
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password rules
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;

    // Two accounts cannot share the same email
    options.User.RequireUniqueEmail = true;

    // Lock the account for 15 minutes after 5 wrong passwords in a row
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
})
.AddEntityFrameworkStores<DcsDb>()   // store users and roles in our database
.AddClaimsPrincipalFactory<ApplicationUserClaimsPrincipalFactory>();   // adds the display name to the login cookie

// Settings for the login cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";               // where visitors are sent when they must sign in
    options.AccessDeniedPath = "/Account/AccessDenied"; // where signed-in users are sent if their role is not allowed
    options.Cookie.HttpOnly = true;                     // JavaScript cannot read the cookie
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // only sent over https
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;                   // stays signed in while the user keeps using the site
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
