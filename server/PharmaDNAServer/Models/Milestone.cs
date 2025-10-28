namespace PharmaDNAServer.Models;

public class Milestone
{
    public int Id { get; set; }
    public int NftId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Location { get; set; }
    public DateTime Timestamp { get; set; }
    public string ActorAddress { get; set; } = string.Empty;
}

