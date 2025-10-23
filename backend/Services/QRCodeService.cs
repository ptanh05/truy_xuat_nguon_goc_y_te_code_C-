using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PharmaDNA.Data;
using PharmaDNA.Models;
using QRCoder;
using QRCodeData = PharmaDNA.Models.QRCodeData;

namespace PharmaDNA.Services
{
    public class QRCodeService : IQRCodeService
    {
        private readonly PharmaDNAContext _context;

        public QRCodeService(PharmaDNAContext context)
        {
            _context = context;
        }

        public async Task<QRCodeData> GenerateQRCodeAsync(int nftId)
        {
            var nft = await _context.NFTs.FindAsync(nftId);
            if (nft == null) return new QRCodeData();

            var existingQR = await _context.QRCodeData
                .FirstOrDefaultAsync(q => q.NFTId == nftId && q.Format == "QR");

            if (existingQR != null)
                return existingQR;

            var qrContent = $"NFT:{nft.Id}|Batch:{nft.BatchId}|Product:{nft.ProductCode}|Mfg:{nft.Manufacturer}";
            
            using (var qrGenerator = new QRCodeGenerator())
            {
                var qrCodeData = qrGenerator.CreateQrCode(qrContent, QRCodeGenerator.ECCLevel.Q);
                // using (var qrCode = new QRCode(qrCodeData))
                // {
                //     var qrImage = qrCode.GetGraphic(20);
                //     var imageBase64 = Convert.ToBase64String(qrImage);
                //     var imageUrl = $"data:image/png;base64,{imageBase64}";
                var imageUrl = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg=="; // Placeholder

                    var qrRecord = new QRCodeData
                    {
                        NFTId = nftId,
                        QRCodeContent = qrContent,
                        QRCodeImageUrl = imageUrl,
                        Format = "QR",
                        IsActive = true
                    };

                    _context.QRCodeData.Add(qrRecord);
                    await _context.SaveChangesAsync();
                    return qrRecord;
                // }
            }
        }

        public async Task<QRCodeData> GenerateBarcodeAsync(int nftId, string format = "Code128")
        {
            var nft = await _context.NFTs.FindAsync(nftId);
            if (nft == null) return null;

            var existingBarcode = await _context.QRCodeData
                .FirstOrDefaultAsync(q => q.NFTId == nftId && q.Format == format);

            if (existingBarcode != null)
                return existingBarcode;

            var barcodeContent = nft.BatchId;

            // Placeholder for barcode generation - use BarcodeLib or similar
            var barcodeUrl = $"data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==";

            var barcodeRecord = new QRCodeData
            {
                NFTId = nftId,
                BarcodeContent = barcodeContent,
                BarcodeImageUrl = barcodeUrl,
                Format = format,
                IsActive = true
            };

            _context.QRCodeData.Add(barcodeRecord);
            await _context.SaveChangesAsync();
            return barcodeRecord;
        }

        public async Task<QRCodeData> GetQRCodeAsync(int id)
        {
            return await _context.QRCodeData
                .Include(q => q.NFT)
                .FirstOrDefaultAsync(q => q.Id == id);
        }

        public async Task<QRCodeData> GetQRCodeByNFTAsync(int nftId)
        {
            return await _context.QRCodeData
                .FirstOrDefaultAsync(q => q.NFTId == nftId && q.Format == "QR");
        }

        public async Task<byte[]> GetQRCodeImageAsync(int id)
        {
            var qrCode = await GetQRCodeAsync(id);
            if (qrCode?.QRCodeImageUrl == null) return null;

            var base64 = qrCode.QRCodeImageUrl.Replace("data:image/png;base64,", "");
            return Convert.FromBase64String(base64);
        }

        public async Task<byte[]> GetBarcodeImageAsync(int id)
        {
            var barcode = await GetQRCodeAsync(id);
            if (barcode?.BarcodeImageUrl == null) return null;

            var base64 = barcode.BarcodeImageUrl.Replace("data:image/png;base64,", "");
            return Convert.FromBase64String(base64);
        }

        public async Task<QRScanLog> LogScanAsync(int qrCodeId, string scannedBy, string scannedFrom)
        {
            var qrCode = await _context.QRCodeData.FindAsync(qrCodeId);
            if (qrCode == null) return null;

            qrCode.ScanCount++;
            qrCode.LastScannedDate = DateTime.UtcNow;

            var scanLog = new QRScanLog
            {
                QRCodeDataId = qrCodeId,
                ScannedBy = scannedBy,
                ScannedFrom = scannedFrom,
                ScanDate = DateTime.UtcNow
            };

            _context.QRScanLog.Add(scanLog);
            _context.QRCodeData.Update(qrCode);
            await _context.SaveChangesAsync();
            return scanLog;
        }

        public async Task<List<QRScanLog>> GetScanHistoryAsync(int qrCodeId)
        {
            return await _context.QRScanLog
                .Where(s => s.QRCodeDataId == qrCodeId)
                .OrderByDescending(s => s.ScanDate)
                .ToListAsync();
        }

        public async Task<byte[]> GeneratePrintableSheetAsync(List<int> nftIds)
        {
            // Placeholder for generating a printable sheet with multiple QR codes
            var qrCodes = await _context.QRCodeData
                .Where(q => nftIds.Contains(q.NFTId))
                .ToListAsync();

            // This would generate a PDF with all QR codes arranged for printing
            var pdfContent = System.Text.Encoding.UTF8.GetBytes("Printable QR Code Sheet");
            return pdfContent;
        }
    }
}
