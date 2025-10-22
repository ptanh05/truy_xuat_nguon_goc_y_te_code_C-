using System.Collections.Generic;
using System.Threading.Tasks;
using PharmaDNA.Models;

namespace PharmaDNA.Services
{
    public interface IQRCodeService
    {
        Task<QRCodeData> GenerateQRCodeAsync(int nftId);
        Task<QRCodeData> GenerateBarcodeAsync(int nftId, string format = "Code128");
        Task<QRCodeData> GetQRCodeAsync(int id);
        Task<QRCodeData> GetQRCodeByNFTAsync(int nftId);
        Task<byte[]> GetQRCodeImageAsync(int id);
        Task<byte[]> GetBarcodeImageAsync(int id);
        Task<QRScanLog> LogScanAsync(int qrCodeId, string scannedBy, string scannedFrom);
        Task<List<QRScanLog>> GetScanHistoryAsync(int qrCodeId);
        Task<byte[]> GeneratePrintableSheetAsync(List<int> nftIds);
    }
}
