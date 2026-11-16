using DCSProject.Controllers;
using DCSProject.Data;
using DCSProject.Services;
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
