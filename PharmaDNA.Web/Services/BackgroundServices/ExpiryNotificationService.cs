using Microsoft.EntityFrameworkCore;
using PharmaDNA.Web.Data;
using PharmaDNA.Web.Models.Entities;

namespace PharmaDNA.Web.Services.BackgroundServices
{
    public class ExpiryNotificationService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ExpiryNotificationService> _logger;
        private readonly TimeSpan _period = TimeSpan.FromHours(24); // Check daily

        public ExpiryNotificationService(IServiceProvider serviceProvider, ILogger<ExpiryNotificationService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckExpiringDrugs();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while checking expiring drugs");
                }

                await Task.Delay(_period, stoppingToken);
            }
        }

        private async Task CheckExpiringDrugs()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var thirtyDaysFromNow = DateTime.UtcNow.AddDays(30);
            var sevenDaysFromNow = DateTime.UtcNow.AddDays(7);

            // Find drugs expiring in 30 days
            var expiringIn30Days = await context.NFTs
                .Where(n => n.ExpiryDate.HasValue && 
                           n.ExpiryDate.Value <= thirtyDaysFromNow && 
                           n.ExpiryDate.Value > DateTime.UtcNow)
                .ToListAsync();

            // Find drugs expiring in 7 days
            var expiringIn7Days = await context.NFTs
                .Where(n => n.ExpiryDate.HasValue && 
                           n.ExpiryDate.Value <= sevenDaysFromNow && 
                           n.ExpiryDate.Value > DateTime.UtcNow)
                .ToListAsync();

            // Log notifications
            foreach (var nft in expiringIn30Days)
            {
                _logger.LogWarning("Drug batch {BatchNumber} expires in {Days} days on {ExpiryDate}", 
                    nft.BatchNumber, 
                    (nft.ExpiryDate!.Value - DateTime.UtcNow).Days,
                    nft.ExpiryDate.Value.ToString("yyyy-MM-dd"));
            }

            foreach (var nft in expiringIn7Days)
            {
                _logger.LogError("URGENT: Drug batch {BatchNumber} expires in {Days} days on {ExpiryDate}", 
                    nft.BatchNumber, 
                    (nft.ExpiryDate!.Value - DateTime.UtcNow).Days,
                    nft.ExpiryDate.Value.ToString("yyyy-MM-dd"));
            }

            // Add milestones for expiring drugs
            foreach (var nft in expiringIn30Days)
            {
                await AddExpiryMilestone(context, nft, "Cảnh báo hết hạn trong 30 ngày");
            }

            foreach (var nft in expiringIn7Days)
            {
                await AddExpiryMilestone(context, nft, "CẢNH BÁO KHẨN CẤP: Hết hạn trong 7 ngày");
            }
        }

        private async Task AddExpiryMilestone(ApplicationDbContext context, NFT nft, string description)
        {
            // Check if milestone already exists for this expiry warning
            var existingMilestone = await context.Milestones
                .FirstOrDefaultAsync(m => m.NftId == nft.Id && 
                                        m.Type == "Cảnh báo hết hạn" && 
                                        m.Description == description);

            if (existingMilestone == null)
            {
                var milestone = new Milestone
                {
                    NftId = nft.Id,
                    Type = "Cảnh báo hết hạn",
                    Description = description,
                    Location = "Hệ thống",
                    Timestamp = DateTime.UtcNow,
                    ActorAddress = "0x0000000000000000000000000000000000000000" // System address
                };

                context.Milestones.Add(milestone);
                await context.SaveChangesAsync();
            }
        }
    }
}
