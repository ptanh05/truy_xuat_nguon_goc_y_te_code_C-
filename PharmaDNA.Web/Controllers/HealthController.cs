using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaDNA.Web.Data;
using PharmaDNA.Web.Services;

namespace PharmaDNA.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IBlockchainService _blockchainService;
        private readonly IIPFSService _ipfsService;
        private readonly ILogger<HealthController> _logger;

        public HealthController(
            ApplicationDbContext context,
            IBlockchainService blockchainService,
            IIPFSService ipfsService,
            ILogger<HealthController> logger)
        {
            _context = context;
            _blockchainService = blockchainService;
            _ipfsService = ipfsService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<HealthCheckResult>> Get()
        {
            var result = new HealthCheckResult
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow,
                Version = "1.0.0",
                Services = new List<ServiceHealth>()
            };

            // Check Database
            try
            {
                await _context.Database.CanConnectAsync();
                result.Services.Add(new ServiceHealth
                {
                    Name = "Database",
                    Status = "Healthy",
                    ResponseTime = 0
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database health check failed");
                result.Services.Add(new ServiceHealth
                {
                    Name = "Database",
                    Status = "Unhealthy",
                    Error = ex.Message
                });
                result.Status = "Degraded";
            }

            // Check Blockchain
            try
            {
                var startTime = DateTime.UtcNow;
                await _blockchainService.GetRoleAsync("0x0000000000000000000000000000000000000000");
                var responseTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
                
                result.Services.Add(new ServiceHealth
                {
                    Name = "Blockchain",
                    Status = "Healthy",
                    ResponseTime = responseTime
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Blockchain health check failed");
                result.Services.Add(new ServiceHealth
                {
                    Name = "Blockchain",
                    Status = "Unhealthy",
                    Error = ex.Message
                });
                result.Status = "Degraded";
            }

            // Check IPFS
            try
            {
                var startTime = DateTime.UtcNow;
                await _ipfsService.VerifyIPFSHashAsync("QmTest");
                var responseTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
                
                result.Services.Add(new ServiceHealth
                {
                    Name = "IPFS",
                    Status = "Healthy",
                    ResponseTime = responseTime
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "IPFS health check failed");
                result.Services.Add(new ServiceHealth
                {
                    Name = "IPFS",
                    Status = "Unhealthy",
                    Error = ex.Message
                });
                result.Status = "Degraded";
            }

            // Check Memory
            var memoryUsage = GC.GetTotalMemory(false);
            var memoryUsageMB = memoryUsage / 1024 / 1024;
            
            result.Services.Add(new ServiceHealth
            {
                Name = "Memory",
                Status = memoryUsageMB < 500 ? "Healthy" : "Warning",
                ResponseTime = memoryUsageMB,
                Details = $"Memory usage: {memoryUsageMB}MB"
            });

            return Ok(result);
        }

        [HttpGet("ready")]
        public async Task<ActionResult> Ready()
        {
            try
            {
                await _context.Database.CanConnectAsync();
                return Ok(new { Status = "Ready", Timestamp = DateTime.UtcNow });
            }
            catch
            {
                return StatusCode(503, new { Status = "Not Ready", Timestamp = DateTime.UtcNow });
            }
        }

        [HttpGet("live")]
        public ActionResult Live()
        {
            return Ok(new { Status = "Alive", Timestamp = DateTime.UtcNow });
        }
    }

    public class HealthCheckResult
    {
        public string Status { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Version { get; set; } = string.Empty;
        public List<ServiceHealth> Services { get; set; } = new List<ServiceHealth>();
    }

    public class ServiceHealth
    {
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public double ResponseTime { get; set; }
        public string? Error { get; set; }
        public string? Details { get; set; }
    }
}
