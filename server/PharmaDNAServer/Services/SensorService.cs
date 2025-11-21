using PharmaDNAServer.Data;

namespace PharmaDNAServer.Services;

public interface ISensorService
{
    Task<SensorUploadResult> SaveSensorDataAsync(SensorUploadContext context);
}

/// <summary>
/// Placeholder service for future AIoT sensor data integration.
/// Keeps validation + logging centralized even before persistence layer is ready.
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

    public Task<SensorUploadResult> SaveSensorDataAsync(SensorUploadContext context)
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

        _logger.LogInformation("Sensor payload queued for NFT #{NftId} ({Distributor}) - {Bytes} bytes",
            context.NftId, context.DistributorAddress, context.Payload.Length);

        // TODO: Persist to SensorLogs table and trigger downstream processing queue.
        return Task.FromResult(new SensorUploadResult(true, "Payload đã được ghi nhận và sẽ xử lý bất đồng bộ."));
    }
}

public record SensorUploadContext(int NftId, string DistributorAddress, byte[] Payload, string? MimeType);

public record SensorUploadResult(bool Success, string Message);


