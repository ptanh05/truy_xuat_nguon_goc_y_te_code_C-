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

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

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
