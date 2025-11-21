using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using PharmaDNAServer.Data;
using DotNetEnv;

// Load .env file if it exists
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Ensure JSON binding/output uses camelCase so FE snake_case/camelCase inputs map consistently
builder.Services.Configure<JsonOptions>(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
});

// Add CORS
builder.Services.AddCors(options =>
{
    var allowedOrigins = builder.Configuration["CORS_ORIGINS"]?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        ?? new[] { "http://localhost:3000", "http://localhost:3001" };
    
    options.AddPolicy("AllowReactApp",
        policy =>
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});

// Resolve PostgreSQL connection string (Neon.tech)
// Support multiple formats: DATABASE_URL (postgresql://), POSTGRES_CONNECTION, NEON_CONNECTION, or appsettings
string? databaseUrl = builder.Configuration["DATABASE_URL"];
string postgresConnection = 
    builder.Configuration["POSTGRES_CONNECTION"] 
    ?? builder.Configuration["NEON_CONNECTION"]
    ?? (databaseUrl != null ? ConvertPostgresUrlToConnectionString(databaseUrl) : null)
    ?? builder.Configuration.GetConnectionString("PostgresConnection")
    ?? throw new InvalidOperationException("No PostgreSQL connection string found. Please configure DATABASE_URL, POSTGRES_CONNECTION, NEON_CONNECTION, or ConnectionStrings:PostgresConnection.");

// Helper function to convert postgresql:// URL to connection string format
static string? ConvertPostgresUrlToConnectionString(string url)
{
    if (string.IsNullOrWhiteSpace(url)) return null;
    
    try
    {
        var uri = new Uri(url);
        var userInfo = uri.UserInfo.Split(':');
        var username = userInfo.Length > 0 ? Uri.UnescapeDataString(userInfo[0]) : "";
        var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "";
        var host = uri.Host;
        var port = uri.Port > 0 ? uri.Port : 5432;
        var database = uri.AbsolutePath.TrimStart('/');
        
        // Parse query string for SSL mode
        var sslMode = "Require";
        if (!string.IsNullOrEmpty(uri.Query))
        {
            var queryParams = uri.Query.TrimStart('?').Split('&');
            foreach (var param in queryParams)
            {
                var parts = param.Split('=');
                if (parts.Length == 2 && parts[0].Equals("sslmode", StringComparison.OrdinalIgnoreCase))
                {
                    sslMode = Uri.UnescapeDataString(parts[1]);
                    break;
                }
            }
        }
        
        return $"Host={host};Port={port};Database={database};Username={username};Password={password};SSL Mode={sslMode}";
    }
    catch
    {
        return null;
    }
}

// Add DbContext - PostgreSQL (Neon.tech) only
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(postgresConnection));

// Bind contract options from environment/appsettings
builder.Services.Configure<PharmaDNAServer.Models.ContractOptions>(options =>
{
    options.PharmaNftAddress = builder.Configuration["PHARMA_NFT_ADDRESS"] ?? string.Empty;
    options.OwnerPrivateKey = builder.Configuration["OWNER_PRIVATE_KEY"] ?? string.Empty;
    options.RpcUrl = builder.Configuration["PHARMADNA_RPC"] ?? string.Empty;
});

// Register services
builder.Services.AddSingleton<PharmaDNAServer.Services.BlockchainService>();
builder.Services.AddScoped<PharmaDNAServer.Services.IRoleService, PharmaDNAServer.Services.RoleService>();
builder.Services.AddScoped<PharmaDNAServer.Services.IMilestoneService, PharmaDNAServer.Services.MilestoneService>();
builder.Services.AddScoped<PharmaDNAServer.Services.ISensorService, PharmaDNAServer.Services.SensorService>();

var app = builder.Build();

// Auto-run migrations to keep schema in sync (useful on Neon)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


// Use CORS
app.UseCors("AllowReactApp");

app.UseAuthorization();

app.MapControllers();

app.Run();
