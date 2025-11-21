using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaDNAServer.Data;
using PharmaDNAServer.Models;

namespace PharmaDNAServer.Controllers;

[ApiController]
[Route("api/pharmacy")]
public class PharmacyController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PharmacyController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetNFTsInPharmacy()
    {
        var nfts = await _context.NFTs
            .Where(n => n.Status == "in_pharmacy")
            .ToListAsync();
        return Ok(nfts);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateNFT([FromBody] PharmacyUpdateNFTRequest request)
    {
        if (request.Id == 0)
        {
            return BadRequest(new { error = "NFT ID không được để trống" });
        }

        if (string.IsNullOrWhiteSpace(request.Status))
        {
            return BadRequest(new { error = "Trạng thái không được để trống" });
        }

        if (string.IsNullOrWhiteSpace(request.PharmacyAddress))
        {
            return BadRequest(new { error = "Địa chỉ nhà thuốc không được để trống" });
        }

        // Validate address format
        if (!request.PharmacyAddress.StartsWith("0x") || request.PharmacyAddress.Length != 42)
        {
            return BadRequest(new { error = "Địa chỉ nhà thuốc không hợp lệ" });
        }

        // Validate status values
        var validStatuses = new[] { "in_pharmacy", "sold", "expired" };
        if (!validStatuses.Contains(request.Status.ToLower()))
        {
            return BadRequest(new { error = $"Trạng thái không hợp lệ. Các trạng thái hợp lệ: {string.Join(", ", validStatuses)}" });
        }

        var nft = await _context.NFTs.FindAsync(request.Id);
        if (nft == null)
        {
            return NotFound(new { error = "Không tìm thấy NFT" });
        }

        nft.Status = request.Status.ToLower();
        nft.PharmacyAddress = request.PharmacyAddress.ToLower();
        _context.NFTs.Update(nft);
        await _context.SaveChangesAsync();

        return Ok(nft);
    }
}

public class PharmacyUpdateNFTRequest
{
    public int Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PharmacyAddress { get; set; } = string.Empty;
}

