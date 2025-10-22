using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PharmaDNA.Models;
using PharmaDNA.Services;

namespace PharmaDNA.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet("user/{walletAddress}")]
        public async Task<IActionResult> GetNotifications(string walletAddress)
        {
            var notifications = await _notificationService.GetNotificationsByAddressAsync(walletAddress);
            return Ok(notifications);
        }

        [HttpGet("unread/{walletAddress}")]
        public async Task<IActionResult> GetUnreadNotifications(string walletAddress)
        {
            var notifications = await _notificationService.GetUnreadNotificationsAsync(walletAddress);
            return Ok(notifications);
        }

        [HttpGet("unread-count/{walletAddress}")]
        public async Task<IActionResult> GetUnreadCount(string walletAddress)
        {
            var count = await _notificationService.GetUnreadCountAsync(walletAddress);
            return Ok(new { unreadCount = count });
        }

        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var result = await _notificationService.MarkAsReadAsync(id);
            return Ok(new { success = result });
        }

        [HttpPut("mark-all-read/{walletAddress}")]
        public async Task<IActionResult> MarkAllAsRead(string walletAddress)
        {
            var result = await _notificationService.MarkAllAsReadAsync(walletAddress);
            return Ok(new { success = result });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            var result = await _notificationService.DeleteNotificationAsync(id);
            return Ok(new { success = result });
        }

        [HttpDelete("all/{walletAddress}")]
        public async Task<IActionResult> DeleteAllNotifications(string walletAddress)
        {
            var result = await _notificationService.DeleteAllNotificationsAsync(walletAddress);
            return Ok(new { success = result });
        }

        [HttpPost("alert-rules")]
        public async Task<IActionResult> CreateAlertRule([FromBody] AlertRule rule)
        {
            var result = await _notificationService.CreateAlertRuleAsync(rule);
            return Ok(result);
        }

        [HttpGet("alert-rules")]
        public async Task<IActionResult> GetAlertRules()
        {
            var rules = await _notificationService.GetAlertRulesAsync();
            return Ok(rules);
        }

        [HttpPut("alert-rules/{id}")]
        public async Task<IActionResult> UpdateAlertRule(int id, [FromBody] AlertRule rule)
        {
            rule.Id = id;
            await _notificationService.UpdateAlertRuleAsync(rule);
            return Ok(rule);
        }

        [HttpDelete("alert-rules/{id}")]
        public async Task<IActionResult> DeleteAlertRule(int id)
        {
            await _notificationService.DeleteAlertRuleAsync(id);
            return Ok(new { success = true });
        }

        [HttpPost("email-templates")]
        public async Task<IActionResult> CreateEmailTemplate([FromBody] EmailTemplate template)
        {
            var result = await _notificationService.CreateEmailTemplateAsync(template);
            return Ok(result);
        }

        [HttpGet("email-templates")]
        public async Task<IActionResult> GetEmailTemplates()
        {
            var templates = await _notificationService.GetEmailTemplatesAsync();
            return Ok(templates);
        }

        [HttpPost("process-pending")]
        public async Task<IActionResult> ProcessPendingNotifications()
        {
            await _notificationService.ProcessPendingNotificationsAsync();
            return Ok(new { success = true });
        }
    }
}
