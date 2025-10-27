using Microsoft.EntityFrameworkCore;
using PharmaDNA.Web.Data;
using PharmaDNA.Web.Services;
using PharmaDNA.Web.Services.BackgroundServices;
using PharmaDNA.Web.Middleware;
using PharmaDNA.Web.Helpers;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables from .env file
var envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
if (File.Exists(envPath))
{
    foreach (var line in File.ReadAllLines(envPath))
    {
        if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
            continue;
            
        var parts = line.Split('=', 2);
        if (parts.Length == 2)
        {
            Environment.SetEnvironmentVariable(parts[0].Trim(), parts[1].Trim());
        }
    }
}

// Add services to the container
builder.Services.AddControllersWithViews()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
    });

// Add API controllers
builder.Services.AddControllers();

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "PharmaDNA API",
        Version = "v1",
        Description = "API for pharmaceutical supply chain traceability using Blockchain and IPFS",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "PharmaDNA Team",
            Email = "support@pharmadna.com"
        }
    });
});

// Validate all configuration before starting
// TODO: Uncomment when you have real configuration
// try
// {
//     ConfigurationValidator.ValidateAll();
// }
// catch (Exception ex)
// {
//     Console.WriteLine($"❌ Configuration Error: {ex.Message}");
//     Console.WriteLine("Please check your .env file and ensure all required variables are set.");
//     Environment.Exit(1);
// }

// Database configuration with environment variable
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")!;

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Register services
builder.Services.AddScoped<IBlockchainService, BlockchainService>();
builder.Services.AddScoped<IIPFSService, IPFSService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<INFTService, NFTService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ICacheService, MemoryCacheService>();

// HTTP Client for IPFS
builder.Services.AddHttpClient<IIPFSService, IPFSService>();

// Memory Cache
builder.Services.AddMemoryCache();

// Background services
builder.Services.AddHostedService<ExpiryNotificationService>();

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "PharmaDNA API v1");
        c.RoutePrefix = "api-docs";
    });
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Add custom middleware
app.UseRequestLogging();

app.UseRouting();

app.UseAuthorization();

// Map API routes
app.MapControllers();

// Map MVC routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        context.Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while creating the database.");
    }
}

app.Run();
