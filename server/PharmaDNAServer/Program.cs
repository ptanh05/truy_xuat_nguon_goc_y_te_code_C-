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

// Resolve SQL Server connection string: prefer env var SQLSERVER_CONNECTION, fallback to appsettings
string resolvedConnectionString =
    builder.Configuration["SQLSERVER_CONNECTION"]
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? string.Empty;

// Add DbContext (SQL Server)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(resolvedConnectionString));

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


// Use CORS
app.UseCors("AllowReactApp");

app.UseAuthorization();

app.MapControllers();

app.Run();
