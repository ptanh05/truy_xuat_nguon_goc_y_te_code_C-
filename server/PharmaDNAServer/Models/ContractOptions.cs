namespace PharmaDNAServer.Models;

public class ContractOptions
{
    public string PharmaNftAddress { get; set; } = string.Empty; // PHARMA_NFT_ADDRESS
    public string OwnerPrivateKey { get; set; } = string.Empty;  // OWNER_PRIVATE_KEY
    public string RpcUrl { get; set; } = string.Empty;           // Optional: PHARMADNA_RPC
    public long? ChainId { get; set; }
}


