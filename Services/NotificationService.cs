using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PharmaDNA.Data;
using PharmaDNA.Models;

namespace PharmaDNA.Services
{
    public class NotificationService : INotificationService
    {
        private readonly PharmaDNAContext _context;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(PharmaDNAContext context, ILogger<NotificationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Notification> CreateNotificationAsync(Notification notification)
        {
            try
            {
                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Notification created for {notification.RecipientAddress}");
                return notification;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating notification: {ex.Message}");
                throw;
            }
        }

        public async Task<List<Notification>> GetNotificationsByAddressAsync(string walletAddress)
        {
            return await _context.Notifications
                .Where(n => n.RecipientAddress == walletAddress)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Notification>> GetUnreadNotificationsAsync(string walletAddress)
        {
            return await _context.Notifications
                .Where(n => n.RecipientAddress == walletAddress && !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(string walletAddress)
        {
            return await _context.Notifications
                .CountAsync(n => n.RecipientAddress == walletAddress && !n.IsRead);
        }

        public async Task<bool> MarkAsReadAsync(int notificationId)
        {
            try
            {
                var notification = await _context.Notifications.FindAsync(notificationId);
                if (notification == null) return false;

                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
                _context.Notifications.Update(notification);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error marking notification as read: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> MarkAllAsReadAsync(string walletAddress)
        {
            try
            {
                var notifications = await _context.Notifications
                    .Where(n => n.RecipientAddress == walletAddress && !n.IsRead)
                    .ToListAsync();

                foreach (var notification in notifications)
                {
                    notification.IsRead = true;
                    notification.ReadAt = DateTime.UtcNow;
                }

                _context.Notifications.UpdateRange(notifications);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error marking all notifications as read: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeleteNotificationAsync(int notificationId)
        {
            try
            {
                var notification = await _context.Notifications.FindAsync(notificationId);
                if (notification == null) return false;

                _context.Notifications.Remove(notification);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting notification: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeleteAllNotificationsAsync(string walletAddress)
        {
            try
            {
                var notifications = await _context.Notifications
                    .Where(n => n.RecipientAddress == walletAddress)
                    .ToListAsync();

                _context.Notifications.RemoveRange(notifications);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting all notifications: {ex.Message}");
                throw;
            }
        }

        public async Task<AlertRule> CreateAlertRuleAsync(AlertRule rule)
        {
            _context.AlertRules.Add(rule);
            await _context.SaveChangesAsync();
            return rule;
        }

        public async Task<List<AlertRule>> GetAlertRulesAsync()
        {
            return await _context.AlertRules
                .Where(r => r.IsActive)
                .ToListAsync();
        }

        public async Task<AlertRule> GetAlertRuleAsync(int id)
        {
            return await _context.AlertRules.FindAsync(id);
        }

        public async Task UpdateAlertRuleAsync(AlertRule rule)
        {
            rule.UpdatedDate = DateTime.UtcNow;
            _context.AlertRules.Update(rule);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAlertRuleAsync(int id)
        {
            var rule = await _context.AlertRules.FindAsync(id);
            if (rule != null)
            {
                _context.AlertRules.Remove(rule);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<EmailTemplate> CreateEmailTemplateAsync(EmailTemplate template)
        {
            _context.EmailTemplates.Add(template);
            await _context.SaveChangesAsync();
            return template;
        }

        public async Task<List<EmailTemplate>> GetEmailTemplatesAsync()
        {
            return await _context.EmailTemplates
                .Where(t => t.IsActive)
                .ToListAsync();
        }

        public async Task<EmailTemplate> GetEmailTemplateAsync(int id)
        {
            return await _context.EmailTemplates.FindAsync(id);
        }

        public async Task UpdateEmailTemplateAsync(EmailTemplate template)
        {
            _context.EmailTemplates.Update(template);
            await _context.SaveChangesAsync();
        }

        public async Task SendEmailNotificationAsync(string email, string subject, string body)
        {
            try
            {
                // Placeholder for email sending - integrate with SendGrid, SMTP, etc.
                _logger.LogInformation($"Email sent to {email}: {subject}");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending email: {ex.Message}");
                throw;
            }
        }

        public async Task SendSMSNotificationAsync(string phoneNumber, string message)
        {
            try
            {
                // Placeholder for SMS sending - integrate with Twilio, etc.
                _logger.LogInformation($"SMS sent to {phoneNumber}: {message}");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending SMS: {ex.Message}");
                throw;
            }
        }

        public async Task SendInAppNotificationAsync(string walletAddress, string title, string message, string type)
        {
            var notification = new Notification
            {
                RecipientAddress = walletAddress,
                Title = title,
                Message = message,
                Type = type,
                Channel = "InApp",
                Priority = "Medium",
                IsSent = true,
                SentAt = DateTime.UtcNow
            };

            await CreateNotificationAsync(notification);
        }

        public async Task ProcessPendingNotificationsAsync()
        {
            var pendingNotifications = await _context.Notifications
                .Where(n => !n.IsSent)
                .ToListAsync();

            foreach (var notification in pendingNotifications)
            {
                try
                {
                    if (notification.Channel == "Email" && !string.IsNullOrEmpty(notification.RecipientEmail))
                    {
                        await SendEmailNotificationAsync(notification.RecipientEmail, notification.Title, notification.Message);
                    }
                    else if (notification.Channel == "SMS")
                    {
                        await SendSMSNotificationAsync(notification.RecipientAddress, notification.Message);
                    }

                    notification.IsSent = true;
                    notification.SentAt = DateTime.UtcNow;
                    _context.Notifications.Update(notification);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error processing notification {notification.Id}: {ex.Message}");
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task TriggerAlertAsync(string triggerType, Dictionary<string, object> data)
        {
            var rules = await GetAlertRulesAsync();
            var matchingRules = rules.Where(r => r.TriggerType == triggerType).ToList();

            foreach (var rule in matchingRules)
            {
                var channels = rule.NotificationChannels.Split(',').Select(c => c.Trim()).ToList();
                var message = $"Alert: {rule.RuleName}";
                var dataJson = JsonSerializer.Serialize(data);

                foreach (var channel in channels)
                {
                    var notification = new Notification
                    {
                        Title = rule.RuleName,
                        Message = message,
                        Type = rule.NotificationType,
                        Channel = channel,
                        Priority = "High",
                        Data = dataJson,
                        IsSent = false
                    };

                    await CreateNotificationAsync(notification);
                }
            }
        }
    }
}
