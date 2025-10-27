namespace PharmaDNA.Web.Services
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true);
        Task<bool> SendExpiryNotificationAsync(string to, string batchNumber, int daysUntilExpiry);
        Task<bool> SendTransferNotificationAsync(string to, string batchNumber, string fromRole, string toRole);
    }
}
