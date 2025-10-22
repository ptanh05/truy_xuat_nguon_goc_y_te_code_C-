using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PharmaDNA.Models;
using PharmaDNA.Services;

namespace PharmaDNA.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QRCodeController : ControllerBase
    {
        private readonly IQRCodeService _qrCodeService;

        public QRCodeController(IQRCodeService qrCodeService)
        {
            _qrCodeService = qrCodeService;
        }

        [HttpPost("generate/{nftId}")]
        public async Task<IActionResult> GenerateQRCode(int nftId)
        {
            var qrCode = await _qrCodeService.GenerateQRCodeAsync(nftId);
            return Ok(qrCode);
        }

        [HttpPost("barcode/{nftId}")]
        public async Task<IActionResult> GenerateBarcode(int nftId, [FromQuery] string format = "Code128")
        {
            var barcode = await _qrCodeService.GenerateBarcodeAsync(nftId, format);
            return Ok(barcode);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetQRCode(int id)
        {
            var qrCode = await _qrCodeService.GetQRCodeAsync(id);
            return Ok(qrCode);
        }

        [HttpGet("nft/{nftId}")]
        public async Task<IActionResult> GetQRCodeByNFT(int nftId)
        {
            var qrCode = await _qrCodeService.GetQRCodeByNFTAsync(nftId);
            return Ok(qrCode);
        }

        [HttpPost("scan/{qrCodeId}")]
        public async Task<IActionResult> LogScan(int qrCodeId, [FromQuery] string scannedBy, [FromQuery] string scannedFrom)
        {
            var scanLog = await _qrCodeService.LogScanAsync(qrCodeId, scannedBy, scannedFrom);
            return Ok(scanLog);
        }

        [HttpGet("history/{qrCodeId}")]
        public async Task<IActionResult> GetScanHistory(int qrCodeId)
        {
            var history = await _qrCodeService.GetScanHistoryAsync(qrCodeId);
            return Ok(history);
        }

        [HttpPost("print")]
        public async Task<IActionResult> GeneratePrintableSheet([FromBody] List<int> nftIds)
        {
            var pdfContent = await _qrCodeService.GeneratePrintableSheetAsync(nftIds);
            return File(pdfContent, "application/pdf", "qr-codes.pdf");
        }
    }
}
