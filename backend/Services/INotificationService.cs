using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PharmaDNA.Models;

namespace PharmaDNA.Services
{
    public interface INotificationService
    {
        // Notification Management
        Task<Notification> CreateNotificationAsync(Notification notification);
        Task<List<Notification>> GetNotificationsByAddressAsync(string walletAddress);
        Task<List<Notification>> GetUnreadNotificationsAsync(string walletAddress);
        Task<int> GetUnreadCountAsync(string walletAddress);
        Task<bool> MarkAsReadAsync(int notificationId);
        Task<bool> MarkAllAsReadAsync(string walletAddress);
        Task<bool> DeleteNotificationAsync(int notificationId);
        Task<bool> DeleteAllNotificationsAsync(string walletAddress);

        // Alert Rules
        Task<AlertRule> CreateAlertRuleAsync(AlertRule rule);
        Task<List<AlertRule>> GetAlertRulesAsync();
        Task<AlertRule> GetAlertRuleAsync(int id);
        Task UpdateAlertRuleAsync(AlertRule rule);
        Task DeleteAlertRuleAsync(int id);

        // Email Templates
        Task<EmailTemplate> CreateEmailTemplateAsync(EmailTemplate template);
        Task<List<EmailTemplate>> GetEmailTemplatesAsync();
        Task<EmailTemplate> GetEmailTemplateAsync(int id);
        Task UpdateEmailTemplateAsync(EmailTemplate template);

        // Sending Notifications
        Task SendEmailNotificationAsync(string email, string subject, string body);
        Task SendSMSNotificationAsync(string phoneNumber, string message);
        Task SendInAppNotificationAsync(string walletAddress, string title, string message, string type);
        Task ProcessPendingNotificationsAsync();
        Task TriggerAlertAsync(string triggerType, Dictionary<string, object> data);
    }
}
