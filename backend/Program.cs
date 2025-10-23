using Microsoft.EntityFrameworkCore;
using PharmaDNA.Data;
using PharmaDNA.Services;
// using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Environment variable overrides for Pharma Network
var pharmaRpcUrl = Environment.GetEnvironmentVariable("PHARMA_RPC_URL");
var pharmaContractAddress = Environment.GetEnvironmentVariable("PHARMA_CONTRACT_ADDRESS");
var pharmaPrivateKey = Environment.GetEnvironmentVariable("PHARMA_PRIVATE_KEY");

if (!string.IsNullOrWhiteSpace(pharmaRpcUrl))
{
    builder.Configuration["PharmaNetwork:RpcUrl"] = pharmaRpcUrl;
}
if (!string.IsNullOrWhiteSpace(pharmaContractAddress))
{
    builder.Configuration["PharmaNetwork:ContractAddress"] = pharmaContractAddress;
}
if (!string.IsNullOrWhiteSpace(pharmaPrivateKey))
{
    builder.Configuration["PharmaNetwork:PrivateKey"] = pharmaPrivateKey;
}

var pinataApiKey = Environment.GetEnvironmentVariable("PINATA_API_KEY");
var pinataApiSecret = Environment.GetEnvironmentVariable("PINATA_SECRET_API_KEY");
var pinataJwt = Environment.GetEnvironmentVariable("PINATA_JWT");
var pinataGateway = Environment.GetEnvironmentVariable("PINATA_GATEWAY");
if (!string.IsNullOrWhiteSpace(pinataApiKey)) builder.Configuration["Pinata:ApiKey"] = pinataApiKey;
if (!string.IsNullOrWhiteSpace(pinataApiSecret)) builder.Configuration["Pinata:ApiSecret"] = pinataApiSecret;
if (!string.IsNullOrWhiteSpace(pinataJwt)) builder.Configuration["Pinata:JwtToken"] = pinataJwt;
if (!string.IsNullOrWhiteSpace(pinataGateway)) builder.Configuration["Pinata:GatewayUrl"] = pinataGateway;

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();

// Database configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<PharmaDNAContext>(options =>
    options.UseSqlServer(connectionString));

// Add custom services
builder.Services.AddScoped<IPharmaNetworkService, PharmaNetworkService>();
builder.Services.AddScoped<IPinataService, PinataService>();

// Add session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseSession();

app.MapControllers();

app.Run();
