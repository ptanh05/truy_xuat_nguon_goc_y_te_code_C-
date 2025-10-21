using Microsoft.EntityFrameworkCore;
using PharmaDNA.Data;
using PharmaDNA.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Database configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<PharmaDNAContext>(options =>
    options.UseSqlServer(connectionString));

// Add custom services
builder.Services.AddScoped<IBlockchainService, BlockchainService>();
builder.Services.AddScoped<IPinataService, PinataService>();
builder.Services.AddScoped<INFTService, NFTService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<Web3ContractService>();

// Add session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseMiddleware<PharmaDNA.Middleware.Web3AuthenticationMiddleware>();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
