using Microsoft.EntityFrameworkCore;
using PharmaDNA.Web.Data;
using PharmaDNA.Web.Models.DTOs;
using PharmaDNA.Web.Models.ViewModels;

namespace PharmaDNA.Web.Services
{
    public class NFTService : INFTService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<NFTService> _logger;

        public NFTService(ApplicationDbContext context, ILogger<NFTService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<int> CreateNFTAsync(NFTDto nftDto)
        {
            try
            {
                var nft = new Models.Entities.NFT
                {
                    Name = nftDto.Name,
                    BatchNumber = nftDto.BatchNumber,
                    Status = nftDto.Status,
                    ManufacturerAddress = nftDto.ManufacturerAddress,
                    DistributorAddress = nftDto.DistributorAddress,
                    PharmacyAddress = nftDto.PharmacyAddress,
                    IpfsHash = nftDto.IpfsHash,
                    CreatedAt = nftDto.CreatedAt,
                    ManufactureDate = nftDto.ManufactureDate,
                    ExpiryDate = nftDto.ExpiryDate,
                    Description = nftDto.Description,
                    ImageUrl = nftDto.ImageUrl
                };

                _context.NFTs.Add(nft);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"NFT created with ID: {nft.Id}");
                return nft.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating NFT");
                throw;
            }
        }

        public async Task<NFTDto?> GetNFTByIdAsync(int id)
        {
            try
            {
                var nft = await _context.NFTs.FindAsync(id);
                return nft != null ? MapToDto(nft) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting NFT by ID: {id}");
                return null;
            }
        }

        public async Task<NFTDto?> GetNFTByBatchNumberAsync(string batchNumber)
        {
            try
            {
                var nft = await _context.NFTs
                    .FirstOrDefaultAsync(n => n.BatchNumber == batchNumber);
                return nft != null ? MapToDto(nft) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting NFT by batch number: {batchNumber}");
                return null;
            }
        }

        public async Task<List<NFTDto>> GetNFTsByStatusAsync(string status)
        {
            try
            {
                var nfts = await _context.NFTs
                    .Where(n => n.Status == status)
                    .ToListAsync();
                return nfts.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting NFTs by status: {status}");
                return new List<NFTDto>();
            }
        }

        public async Task<List<NFTDto>> GetAllNFTsAsync()
        {
            try
            {
                var nfts = await _context.NFTs.ToListAsync();
                return nfts.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all NFTs");
                return new List<NFTDto>();
            }
        }

        public async Task<bool> UpdateNFTStatusAsync(int id, string status)
        {
            try
            {
                var nft = await _context.NFTs.FindAsync(id);
                if (nft == null) return false;

                nft.Status = status;
                await _context.SaveChangesAsync();

                _logger.LogInformation($"NFT {id} status updated to {status}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating NFT {id} status");
                return false;
            }
        }

        public async Task<bool> UpdateNFTDistributorAsync(int id, string distributorAddress)
        {
            try
            {
                var nft = await _context.NFTs.FindAsync(id);
                if (nft == null) return false;

                nft.DistributorAddress = distributorAddress;
                nft.Status = "in_transit";
                await _context.SaveChangesAsync();

                _logger.LogInformation($"NFT {id} distributor updated to {distributorAddress}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating NFT {id} distributor");
                return false;
            }
        }

        public async Task<bool> UpdateNFTPharmacyAsync(int id, string pharmacyAddress)
        {
            try
            {
                var nft = await _context.NFTs.FindAsync(id);
                if (nft == null) return false;

                nft.PharmacyAddress = pharmacyAddress;
                nft.Status = "in_pharmacy";
                await _context.SaveChangesAsync();

                _logger.LogInformation($"NFT {id} pharmacy updated to {pharmacyAddress}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating NFT {id} pharmacy");
                return false;
            }
        }

        public async Task<int> CreateTransferRequestAsync(int nftId, string distributorAddress)
        {
            try
            {
                var transferRequest = new Models.Entities.TransferRequest
                {
                    NftId = nftId,
                    DistributorAddress = distributorAddress,
                    PharmacyAddress = "", // Will be set when pharmacy accepts
                    Status = "pending"
                };

                _context.TransferRequests.Add(transferRequest);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Transfer request created for NFT {nftId}");
                return transferRequest.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating transfer request for NFT {nftId}");
                throw;
            }
        }

        public async Task<List<TransferRequestDto>> GetTransferRequestsAsync()
        {
            try
            {
                var requests = await _context.TransferRequests
                    .Include(tr => tr.NFT)
                    .OrderByDescending(tr => tr.CreatedAt)
                    .ToListAsync();

                return requests.Select(tr => new TransferRequestDto
                {
                    Id = tr.Id,
                    NftId = tr.NftId,
                    DistributorAddress = tr.DistributorAddress,
                    PharmacyAddress = tr.PharmacyAddress,
                    TransferNote = tr.TransferNote,
                    Status = tr.Status,
                    CreatedAt = tr.CreatedAt,
                    UpdatedAt = tr.UpdatedAt
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transfer requests");
                return new List<TransferRequestDto>();
            }
        }

        public async Task<bool> ApproveTransferRequestAsync(int requestId, int nftId, string distributorAddress)
        {
            try
            {
                var request = await _context.TransferRequests.FindAsync(requestId);
                if (request == null) return false;

                request.Status = "approved";
                request.UpdatedAt = DateTime.UtcNow;

                await UpdateNFTDistributorAsync(nftId, distributorAddress);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Transfer request {requestId} approved");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error approving transfer request {requestId}");
                return false;
            }
        }

        public async Task<bool> AddMilestoneAsync(MilestoneViewModel milestone)
        {
            try
            {
                var milestoneEntity = new Models.Entities.Milestone
                {
                    NftId = milestone.NftId,
                    Type = milestone.Type,
                    Description = milestone.Description,
                    Location = milestone.Location,
                    Timestamp = DateTime.UtcNow,
                    ActorAddress = milestone.ActorAddress
                };

                _context.Milestones.Add(milestoneEntity);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Milestone added for NFT {milestone.NftId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding milestone for NFT {milestone.NftId}");
                return false;
            }
        }

        public async Task<List<MilestoneInfo>> GetMilestonesByNftIdAsync(int nftId)
        {
            try
            {
                var milestones = await _context.Milestones
                    .Where(m => m.NftId == nftId)
                    .OrderBy(m => m.Timestamp)
                    .ToListAsync();

                return milestones.Select(m => new MilestoneInfo
                {
                    Id = m.Id,
                    Type = m.Type,
                    Description = m.Description,
                    Location = m.Location,
                    Timestamp = m.Timestamp,
                    ActorAddress = m.ActorAddress
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting milestones for NFT {nftId}");
                return new List<MilestoneInfo>();
            }
        }

        public async Task<List<MilestoneInfo>> GetMilestonesByBatchNumberAsync(string batchNumber)
        {
            try
            {
                var nft = await _context.NFTs
                    .FirstOrDefaultAsync(n => n.BatchNumber == batchNumber);
                
                if (nft == null) return new List<MilestoneInfo>();

                return await GetMilestonesByNftIdAsync(nft.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting milestones for batch {batchNumber}");
                return new List<MilestoneInfo>();
            }
        }

        public async Task<bool> UpdateSensorDataAsync(int nftId, string sensorIpfsHash)
        {
            try
            {
                var nft = await _context.NFTs.FindAsync(nftId);
                if (nft == null) return false;

                // Update NFT with sensor data hash
                // This could be stored in a separate field or as part of the description
                nft.Description += $"\n[Sensor Data: {sensorIpfsHash}]";
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Sensor data updated for NFT {nftId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating sensor data for NFT {nftId}");
                return false;
            }
        }

        private NFTDto MapToDto(Models.Entities.NFT nft)
        {
            return new NFTDto
            {
                Id = nft.Id,
                Name = nft.Name,
                BatchNumber = nft.BatchNumber,
                Status = nft.Status,
                ManufacturerAddress = nft.ManufacturerAddress,
                DistributorAddress = nft.DistributorAddress,
                PharmacyAddress = nft.PharmacyAddress,
                IpfsHash = nft.IpfsHash,
                CreatedAt = nft.CreatedAt,
                ManufactureDate = nft.ManufactureDate,
                ExpiryDate = nft.ExpiryDate,
                Description = nft.Description,
                ImageUrl = nft.ImageUrl
            };
        }
    }
}
