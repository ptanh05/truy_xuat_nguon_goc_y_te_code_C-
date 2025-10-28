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
    public async Task<IActionResult> UpdateNFT([FromBody] UpdateNFTRequest request)
    {
        if (request.Id == 0 || string.IsNullOrEmpty(request.Status) || string.IsNullOrEmpty(request.PharmacyAddress))
        {
            return BadRequest(new { error = "Thiếu thông tin" });
        }

        var nft = await _context.NFTs.FindAsync(request.Id);
        if (nft == null)
        {
            return NotFound(new { error = "Không tìm thấy NFT" });
        }

        nft.Status = request.Status;
        nft.PharmacyAddress = request.PharmacyAddress;
        _context.NFTs.Update(nft);
        await _context.SaveChangesAsync();

        return Ok(nft);
    }
}

public class UpdateNFTRequest
{
    public int Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PharmacyAddress { get; set; } = string.Empty;
}

