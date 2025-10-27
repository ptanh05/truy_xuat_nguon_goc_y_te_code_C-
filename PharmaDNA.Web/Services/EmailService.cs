using System.Net;
using System.Net.Mail;

namespace PharmaDNA.Web.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly IConfiguration _configuration;

        public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true)
        {
            try
            {
                var smtpHost = Environment.GetEnvironmentVariable("EMAIL_SMTP_HOST") ?? "smtp.gmail.com";
                var smtpPort = int.Parse(Environment.GetEnvironmentVariable("EMAIL_SMTP_PORT") ?? "587");
                var smtpUser = Environment.GetEnvironmentVariable("EMAIL_SMTP_USER");
                var smtpPass = Environment.GetEnvironmentVariable("EMAIL_SMTP_PASS");

                if (string.IsNullOrEmpty(smtpUser) || string.IsNullOrEmpty(smtpPass))
                {
                    _logger.LogWarning("Email configuration not found, skipping email send");
                    return false;
                }

                using var client = new SmtpClient(smtpHost, smtpPort)
                {
                    Credentials = new NetworkCredential(smtpUser, smtpPass),
                    EnableSsl = true
                };

                using var message = new MailMessage
                {
                    From = new MailAddress(smtpUser, "PharmaDNA System"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = isHtml
                };

                message.To.Add(to);

                await client.SendMailAsync(message);
                _logger.LogInformation($"Email sent successfully to {to}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending email to {to}");
                return false;
            }
        }

        public async Task<bool> SendExpiryNotificationAsync(string to, string batchNumber, int daysUntilExpiry)
        {
            var subject = daysUntilExpiry <= 7 
                ? $"🚨 CẢNH BÁO KHẨN CẤP: Lô thuốc {batchNumber} sắp hết hạn"
                : $"⚠️ Cảnh báo: Lô thuốc {batchNumber} sắp hết hạn";

            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2 style='color: {(daysUntilExpiry <= 7 ? "#dc2626" : "#f59e0b")};'>
                        {(daysUntilExpiry <= 7 ? "🚨 CẢNH BÁO KHẨN CẤP" : "⚠️ Cảnh báo")}
                    </h2>
                    <p>Lô thuốc <strong>{batchNumber}</strong> sẽ hết hạn trong <strong>{daysUntilExpiry} ngày</strong>.</p>
                    <p>Vui lòng kiểm tra và xử lý kịp thời.</p>
                    <div style='background-color: #f3f4f6; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                        <p><strong>Thông tin lô thuốc:</strong></p>
                        <ul>
                            <li>Số lô: {batchNumber}</li>
                            <li>Ngày hết hạn: {DateTime.UtcNow.AddDays(daysUntilExpiry):dd/MM/yyyy}</li>
                            <li>Trạng thái: Cần xử lý</li>
                        </ul>
                    </div>
                    <p>Trân trọng,<br>Hệ thống PharmaDNA</p>
                </div>";

            return await SendEmailAsync(to, subject, body);
        }

        public async Task<bool> SendTransferNotificationAsync(string to, string batchNumber, string fromRole, string toRole)
        {
            var subject = $"📦 Thông báo chuyển lô thuốc {batchNumber}";
            
            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2 style='color: #3b82f6;'>📦 Thông báo chuyển lô thuốc</h2>
                    <p>Lô thuốc <strong>{batchNumber}</strong> đã được chuyển từ <strong>{fromRole}</strong> sang <strong>{toRole}</strong>.</p>
                    <div style='background-color: #f0f9ff; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                        <p><strong>Chi tiết chuyển lô:</strong></p>
                        <ul>
                            <li>Số lô: {batchNumber}</li>
                            <li>Từ: {fromRole}</li>
                            <li>Đến: {toRole}</li>
                            <li>Thời gian: {DateTime.UtcNow:dd/MM/yyyy HH:mm}</li>
                        </ul>
                    </div>
                    <p>Vui lòng kiểm tra và xác nhận nhận lô thuốc.</p>
                    <p>Trân trọng,<br>Hệ thống PharmaDNA</p>
                </div>";

            return await SendEmailAsync(to, subject, body);
        }
    }
}
