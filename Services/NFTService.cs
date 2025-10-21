using PharmaDNA.Data;
using PharmaDNA.Models;
using Microsoft.EntityFrameworkCore;

namespace PharmaDNA.Services
{
    public class NFTService : INFTService
    {
        private readonly PharmaDNAContext _context;
        private readonly ILogger<NFTService> _logger;

        public NFTService(PharmaDNAContext context, ILogger<NFTService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<NFT> CreateNFTAsync(NFT nft)
        {
            try
            {
                _context.NFTs.Add(nft);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"NFT created: {nft.BatchNumber}");
                return nft;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating NFT: {ex.Message}");
                throw;
            }
        }

        public async Task<NFT> GetNFTByIdAsync(int id)
        {
            return await _context.NFTs.Include(n => n.Milestones).FirstOrDefaultAsync(n => n.Id == id);
        }

        public async Task<NFT> GetNFTByBatchNumberAsync(string batchNumber)
        {
            return await _context.NFTs.Include(n => n.Milestones).FirstOrDefaultAsync(n => n.BatchNumber == batchNumber);
        }

        public async Task<List<NFT>> GetNFTsByManufacturerAsync(string manufacturerAddress)
        {
            return await _context.NFTs
                .Where(n => n.ManufacturerAddress == manufacturerAddress)
                .Include(n => n.Milestones)
                .ToListAsync();
        }

        public async Task<List<NFT>> GetNFTsByDistributorAsync(string distributorAddress)
        {
            return await _context.NFTs
                .Where(n => n.DistributorAddress == distributorAddress)
                .Include(n => n.Milestones)
                .ToListAsync();
        }

        public async Task<List<NFT>> GetNFTsByPharmacyAsync(string pharmacyAddress)
        {
            return await _context.NFTs
                .Where(n => n.PharmacyAddress == pharmacyAddress)
                .Include(n => n.Milestones)
                .ToListAsync();
        }

        public async Task<NFT> UpdateNFTAsync(NFT nft)
        {
            try
            {
                nft.UpdatedAt = DateTime.UtcNow;
                _context.NFTs.Update(nft);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"NFT updated: {nft.BatchNumber}");
                return nft;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating NFT: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeleteNFTAsync(int id)
        {
            try
            {
                var nft = await _context.NFTs.FindAsync(id);
                if (nft == null) return false;

                _context.NFTs.Remove(nft);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"NFT deleted: {id}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting NFT: {ex.Message}");
                throw;
            }
        }
    }
}
