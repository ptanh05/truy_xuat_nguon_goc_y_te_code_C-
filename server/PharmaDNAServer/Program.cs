using Microsoft.EntityFrameworkCore;
using PharmaDNAServer.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:3000", "http://localhost:3001")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});

// Resolve Postgres connection string: prefer DATABASE_URL env (e.g. Neon), fallback to appsettings
string? databaseUrl = builder.Configuration["DATABASE_URL"];
string resolvedConnectionString;

if (!string.IsNullOrWhiteSpace(databaseUrl) && databaseUrl.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase))
{
    // Parse postgres://user:pass@host:port/db -> Npgsql conn string
    var uri = new Uri(databaseUrl);
    var userInfo = uri.UserInfo.Split(':', 2);
    var username = Uri.UnescapeDataString(userInfo[0]);
    var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty;
    var host = uri.Host;
    var port = uri.Port > 0 ? uri.Port : 5432;
    var database = uri.AbsolutePath.TrimStart('/');
    // Neon requires SSL; trust cert in dev
    resolvedConnectionString =
        $"Host={host};Port={port};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true";
}
else
{
    resolvedConnectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
}

// Add DbContext (PostgreSQL)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(resolvedConnectionString));

// Bind contract options from environment/appsettings
builder.Services.Configure<PharmaDNAServer.Models.ContractOptions>(options =>
{
    options.PharmaNftAddress = builder.Configuration["PHARMA_NFT_ADDRESS"] ?? string.Empty;
    options.OwnerPrivateKey = builder.Configuration["OWNER_PRIVATE_KEY"] ?? string.Empty;
    options.RpcUrl = builder.Configuration["PHARMADNA_RPC"] ?? string.Empty;
});

// Register services
builder.Services.AddSingleton<PharmaDNAServer.Services.BlockchainService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Use CORS
app.UseCors("AllowReactApp");

app.UseAuthorization();

app.MapControllers();

app.Run();
