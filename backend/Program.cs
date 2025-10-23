using Microsoft.EntityFrameworkCore;
using PharmaDNA.Data;
using PharmaDNA.Services;
// using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Environment variable overrides for external services (Infura, Pinata)
// Prefer explicit env vars if present to avoid changing appsettings files across machines
var infuraEndpoint = Environment.GetEnvironmentVariable("INFURA_ENDPOINT");
var infuraProjectId = Environment.GetEnvironmentVariable("INFURA_PROJECT_ID");
if (!string.IsNullOrWhiteSpace(infuraEndpoint))
{
    builder.Configuration["Blockchain:RpcUrl"] = infuraEndpoint;
}
else if (!string.IsNullOrWhiteSpace(infuraProjectId))
{
    // Fallback to standard Infura mainnet endpoint format if only ProjectId is provided
    builder.Configuration["Blockchain:RpcUrl"] = $"https://mainnet.infura.io/v3/{infuraProjectId}";
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
builder.Services.AddRazorPages();
builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();

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

// Additional services
builder.Services.AddScoped<IApiKeyService, ApiKeyService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IBatchOperationService, BatchOperationService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IDisputeService, DisputeService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<ISearchService, SearchService>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<IQRCodeService, QRCodeService>();

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
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseMiddleware<PharmaDNA.Middleware.Web3AuthenticationMiddleware>();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();
app.MapControllers();

app.Run();
