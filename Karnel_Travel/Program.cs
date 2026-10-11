using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Karnel_Travel.Data;

var builder = WebApplication.CreateBuilder(args);

// Database ki connection string appsettings.json se leta hai
var connectionString = builder.Configuration.GetConnectionString("Karnel_TravelContext")
    ?? throw new InvalidOperationException("Connection string 'Karnel_TravelContext' not found.");

// Database context (SQL Server) register karo
builder.Services.AddDbContext<Karnel_TravelContext>(options => options.UseSqlServer(connectionString));

// Login/Register system (Identity) aur Roles (Admin, User) register karo
builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<Karnel_TravelContext>();

// Deactivate
builder.Services.Configure<SecurityStampValidatorOptions>(o => o.ValidationInterval = TimeSpan.Zero);

// Booking system ki service (price, availability, booking banana/cancel karna)
builder.Services.AddScoped<BookingService>();

// MVC (Controllers aur Views) register karo
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Production me error page aur HTTPS security headers
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();   // HTTP ko HTTPS par bhejta hai
app.UseStaticFiles();        // wwwroot ki files (aur admin ki upload ki hui images) serve karta hai
app.UseRouting();            // URL ko controller se match karta hai

app.UseAuthentication();     // pehchan: user kaun hai (login)
app.UseAuthorization();      // ijazat: user ko kya karne ki ijazat hai (Admin/User)

app.MapStaticAssets();

// Admin area ka route (jaise /Admin/Hotels)
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}")
    .WithStaticAssets();

// Normal website ka route (jaise /Home/About)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Identity ke pages (Login, Register waghera)
app.MapRazorPages()
    .WithStaticAssets();

// Roles aur Admin user pehli baar banao
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<Karnel_TravelContext>();
    await db.Database.MigrateAsync();

    await DbSeeder.SeedAsync(scope.ServiceProvider);
}

app.Run();