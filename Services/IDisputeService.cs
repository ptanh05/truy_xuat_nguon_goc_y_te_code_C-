using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PharmaDNA.Models;

namespace PharmaDNA.Services
{
    public interface IDisputeService
    {
        // Dispute Management
        Task<Dispute> CreateDisputeAsync(Dispute dispute);
        Task<Dispute> GetDisputeAsync(int disputeId);
        Task<IEnumerable<Dispute>> GetAllDisputesAsync(int pageNumber = 1, int pageSize = 10);
        Task<IEnumerable<Dispute>> GetDisputesByStatusAsync(string status, int pageNumber = 1, int pageSize = 10);
        Task<IEnumerable<Dispute>> GetDisputesByUserAsync(int userId, int pageNumber = 1, int pageSize = 10);
        Task<bool> UpdateDisputeStatusAsync(int disputeId, string status);
        Task<bool> AssignDisputeAsync(int disputeId, int userId);
        Task<bool> UpdateDisputeAsync(Dispute dispute);

        // Dispute Comments
        Task<bool> AddCommentAsync(int disputeId, int userId, string comment, bool isInternal = false);
        Task<IEnumerable<DisputeComment>> GetDisputeCommentsAsync(int disputeId);

        // Dispute Resolution
        Task<DisputeResolution> CreateResolutionAsync(DisputeResolution resolution);
        Task<IEnumerable<DisputeResolution>> GetDisputeResolutionsAsync(int disputeId);
        Task<bool> ApproveResolutionAsync(int resolutionId);
        Task<bool> RejectResolutionAsync(int resolutionId);

        // Statistics
        Task<DisputeStatistics> GetDisputeStatisticsAsync();
        Task<IEnumerable<DisputeByType>> GetDisputesByTypeAsync();
    }
}
