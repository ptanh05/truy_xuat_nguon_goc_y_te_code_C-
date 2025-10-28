using Microsoft.Extensions.Options;

namespace PharmaDNAServer.Services;

public class BlockchainService
{
    private readonly Models.ContractOptions _options;

    public BlockchainService(IOptions<Models.ContractOptions> options)
    {
        _options = options.Value;
    }

    public bool IsConfigured()
    {
        return !string.IsNullOrWhiteSpace(_options.PharmaNftAddress)
            && !string.IsNullOrWhiteSpace(_options.OwnerPrivateKey);
    }
}


