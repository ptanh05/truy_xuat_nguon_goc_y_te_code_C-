using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PharmaDNA.Data;
using PharmaDNA.Models;

namespace PharmaDNA.Services
{
    public class DisputeService : IDisputeService
    {
        private readonly PharmaDNAContext _context;

        public DisputeService(PharmaDNAContext context)
        {
            _context = context;
        }

        public async Task<Dispute> CreateDisputeAsync(Dispute dispute)
        {
            dispute.CreatedAt = DateTime.UtcNow;
            dispute.Status = "Open";
            dispute.Priority = dispute.Priority ?? "Medium";

            _context.Disputes.Add(dispute);
            await _context.SaveChangesAsync();
            return dispute;
        }

        public async Task<Dispute> GetDisputeAsync(int disputeId)
        {
            return await _context.Disputes
                .Include(d => d.NFT)
                .Include(d => d.ReportedByUser)
                .Include(d => d.AssignedToUser)
                .Include(d => d.Comments)
                .Include(d => d.Resolutions)
                .FirstOrDefaultAsync(d => d.Id == disputeId);
        }

        public async Task<IEnumerable<Dispute>> GetAllDisputesAsync(int pageNumber = 1, int pageSize = 10)
        {
            return await _context.Disputes
                .Include(d => d.NFT)
                .Include(d => d.ReportedByUser)
                .OrderByDescending(d => d.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Dispute>> GetDisputesByStatusAsync(string status, int pageNumber = 1, int pageSize = 10)
        {
            return await _context.Disputes
                .Where(d => d.Status == status)
                .Include(d => d.NFT)
                .Include(d => d.ReportedByUser)
                .OrderByDescending(d => d.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Dispute>> GetDisputesByUserAsync(int userId, int pageNumber = 1, int pageSize = 10)
        {
            return await _context.Disputes
                .Where(d => d.ReportedByUserId == userId.ToString())
                .Include(d => d.NFT)
                .OrderByDescending(d => d.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<bool> UpdateDisputeStatusAsync(int disputeId, string status)
        {
            var dispute = await _context.Disputes.FindAsync(disputeId);
            if (dispute == null) return false;

            dispute.Status = status;
            if (status == "Resolved" || status == "Closed")
                dispute.ResolvedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AssignDisputeAsync(int disputeId, int userId)
        {
            var dispute = await _context.Disputes.FindAsync(disputeId);
            if (dispute == null) return false;

            dispute.AssignedToUserId = userId.ToString();
            dispute.Status = "InProgress";
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateDisputeAsync(Dispute dispute)
        {
            _context.Disputes.Update(dispute);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AddCommentAsync(int disputeId, int userId, string comment, bool isInternal = false)
        {
            var disputeComment = new DisputeComment
            {
                DisputeId = disputeId,
                UserId = userId.ToString(),
                Comment = comment,
                IsInternal = isInternal,
                CreatedAt = DateTime.UtcNow
            };

            _context.DisputeComments.Add(disputeComment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<DisputeComment>> GetDisputeCommentsAsync(int disputeId)
        {
            return await _context.DisputeComments
                .Where(c => c.DisputeId == disputeId)
                .Include(c => c.User)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<DisputeResolution> CreateResolutionAsync(DisputeResolution resolution)
        {
            resolution.CreatedAt = DateTime.UtcNow;
            resolution.Status = "Pending";

            _context.DisputeResolutions.Add(resolution);
            await _context.SaveChangesAsync();
            return resolution;
        }

        public async Task<IEnumerable<DisputeResolution>> GetDisputeResolutionsAsync(int disputeId)
        {
            return await _context.DisputeResolutions
                .Where(r => r.DisputeId == disputeId)
                .Include(r => r.ResolvedByUser)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> ApproveResolutionAsync(int resolutionId)
        {
            var resolution = await _context.DisputeResolutions.FindAsync(resolutionId);
            if (resolution == null) return false;

            resolution.Status = "Approved";
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RejectResolutionAsync(int resolutionId)
        {
            var resolution = await _context.DisputeResolutions.FindAsync(resolutionId);
            if (resolution == null) return false;

            resolution.Status = "Rejected";
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<DisputeStatistics> GetDisputeStatisticsAsync()
        {
            var totalDisputes = await _context.Disputes.CountAsync();
            var openDisputes = await _context.Disputes.CountAsync(d => d.Status == "Open");
            var resolvedDisputes = await _context.Disputes.CountAsync(d => d.Status == "Resolved");
            var totalCompensation = await _context.Disputes.SumAsync(d => d.CompensationAmount);

            return new DisputeStatistics
            {
                TotalDisputes = totalDisputes,
                OpenDisputes = openDisputes,
                ResolvedDisputes = resolvedDisputes,
                ResolutionRate = totalDisputes > 0 ? (double)resolvedDisputes / totalDisputes * 100 : 0,
                TotalCompensation = totalCompensation ?? 0
            };
        }

        public async Task<IEnumerable<DisputeByType>> GetDisputesByTypeAsync()
        {
            return await _context.Disputes
                .GroupBy(d => d.DisputeType)
                .Select(g => new DisputeByType
                {
                    Type = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();
        }
    }
}
