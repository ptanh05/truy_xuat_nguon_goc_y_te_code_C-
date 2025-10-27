using Microsoft.EntityFrameworkCore;
using PharmaDNA.Web.Data;
using PharmaDNA.Web.Models.DTOs;
using PharmaDNA.Web.Models.Entities;
using PharmaDNA.Web.Services;
using Xunit;
using FluentAssertions;
using Moq;

namespace PharmaDNA.Web.Tests.Services
{
    public class NFTServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly NFTService _nftService;
        private readonly Mock<ILogger<NFTService>> _mockLogger;

        public NFTServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _mockLogger = new Mock<ILogger<NFTService>>();
            _nftService = new NFTService(_context, _mockLogger.Object);
        }

        [Fact]
        public async Task CreateNFTAsync_ShouldCreateNFT_WhenValidData()
        {
            // Arrange
            var nftDto = new NFTDto
            {
                Name = "Test Drug",
                BatchNumber = "LOT001",
                Status = "CREATED",
                ManufacturerAddress = "0x1234567890123456789012345678901234567890",
                IpfsHash = "QmTestHash",
                CreatedAt = DateTime.UtcNow
            };

            // Act
            var result = await _nftService.CreateNFTAsync(nftDto);

            // Assert
            result.Should().BeGreaterThan(0);
            
            var createdNft = await _context.NFTs.FindAsync(result);
            createdNft.Should().NotBeNull();
            createdNft!.Name.Should().Be(nftDto.Name);
            createdNft.BatchNumber.Should().Be(nftDto.BatchNumber);
        }

        [Fact]
        public async Task GetNFTByBatchNumberAsync_ShouldReturnNFT_WhenExists()
        {
            // Arrange
            var nft = new NFT
            {
                Name = "Test Drug",
                BatchNumber = "LOT001",
                Status = "CREATED",
                ManufacturerAddress = "0x1234567890123456789012345678901234567890",
                CreatedAt = DateTime.UtcNow
            };

            _context.NFTs.Add(nft);
            await _context.SaveChangesAsync();

            // Act
            var result = await _nftService.GetNFTByBatchNumberAsync("LOT001");

            // Assert
            result.Should().NotBeNull();
            result!.BatchNumber.Should().Be("LOT001");
        }

        [Fact]
        public async Task GetNFTByBatchNumberAsync_ShouldReturnNull_WhenNotExists()
        {
            // Act
            var result = await _nftService.GetNFTByBatchNumberAsync("NONEXISTENT");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateNFTStatusAsync_ShouldUpdateStatus_WhenValidId()
        {
            // Arrange
            var nft = new NFT
            {
                Name = "Test Drug",
                BatchNumber = "LOT001",
                Status = "CREATED",
                ManufacturerAddress = "0x1234567890123456789012345678901234567890",
                CreatedAt = DateTime.UtcNow
            };

            _context.NFTs.Add(nft);
            await _context.SaveChangesAsync();

            // Act
            var result = await _nftService.UpdateNFTStatusAsync(nft.Id, "in_transit");

            // Assert
            result.Should().BeTrue();
            
            var updatedNft = await _context.NFTs.FindAsync(nft.Id);
            updatedNft!.Status.Should().Be("in_transit");
        }

        [Fact]
        public async Task CreateTransferRequestAsync_ShouldCreateRequest_WhenValidData()
        {
            // Arrange
            var nft = new NFT
            {
                Name = "Test Drug",
                BatchNumber = "LOT001",
                Status = "CREATED",
                ManufacturerAddress = "0x1234567890123456789012345678901234567890",
                CreatedAt = DateTime.UtcNow
            };

            _context.NFTs.Add(nft);
            await _context.SaveChangesAsync();

            // Act
            var result = await _nftService.CreateTransferRequestAsync(nft.Id, "0x9876543210987654321098765432109876543210");

            // Assert
            result.Should().BeGreaterThan(0);
            
            var transferRequest = await _context.TransferRequests.FindAsync(result);
            transferRequest.Should().NotBeNull();
            transferRequest!.NftId.Should().Be(nft.Id);
            transferRequest.Status.Should().Be("pending");
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
