using Microsoft.EntityFrameworkCore;
using PharmaDNAServer.Data;
using PharmaDNAServer.Models;

namespace PharmaDNAServer.Services;

public interface ISensorService
{
    Task<SensorUploadResult> SaveSensorDataAsync(SensorUploadContext context);
    Task<IEnumerable<SensorLog>> GetSensorLogsAsync(int nftId);
}

/// <summary>
/// Service xử lý dữ liệu cảm biến AIoT từ nhà phân phối
/// Lưu trữ vào database và có thể trigger xử lý bất đồng bộ
/// </summary>
public class SensorService : ISensorService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SensorService> _logger;

    public SensorService(ApplicationDbContext context, ILogger<SensorService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<SensorUploadResult> SaveSensorDataAsync(SensorUploadContext context)
    {
        if (context.NftId <= 0)
        {
            throw new ArgumentException("NFT id không hợp lệ");
        }

        if (string.IsNullOrWhiteSpace(context.DistributorAddress))
        {
            throw new ArgumentException("Thiếu địa chỉ nhà phân phối");
        }

        if (context.Payload == null || context.Payload.Length == 0)
        {
            throw new ArgumentException("Payload cảm biến trống");
        }

        // Kiểm tra NFT có tồn tại không
        var nft = await _context.NFTs.FindAsync(context.NftId);
        if (nft == null)
        {
            throw new ArgumentException($"NFT #{context.NftId} không tồn tại");
        }

        // Tạo SensorLog và lưu vào database
        var sensorLog = new SensorLog
        {
            NftId = context.NftId,
            DistributorAddress = context.DistributorAddress.ToLower(),
            Payload = context.Payload,
            MimeType = context.MimeType,
            CreatedAt = DateTime.UtcNow,
            ProcessedStatus = "pending"
        };

        _context.SensorLogs.Add(sensorLog);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Sensor payload saved for NFT #{NftId} ({Distributor}) - {Bytes} bytes, LogId: {LogId}",
            context.NftId, context.DistributorAddress, context.Payload.Length, sensorLog.Id);

        // TODO: Trigger downstream processing queue (Hangfire, RabbitMQ, Azure Service Bus, etc.)
        // Ví dụ: await _queueService.EnqueueAsync("process-sensor-data", sensorLog.Id);

        return new SensorUploadResult(
            true, 
            $"Payload đã được lưu vào database (LogId: {sensorLog.Id}) và sẽ được xử lý bất đồng bộ.");
    }

    public async Task<IEnumerable<SensorLog>> GetSensorLogsAsync(int nftId)
    {
        return await _context.SensorLogs
            .Where(s => s.NftId == nftId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }
}

public record SensorUploadContext(int NftId, string DistributorAddress, byte[] Payload, string? MimeType);

public record SensorUploadResult(bool Success, string Message);


