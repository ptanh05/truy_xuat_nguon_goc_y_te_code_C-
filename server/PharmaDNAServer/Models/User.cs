namespace PharmaDNAServer.Models;

public class User
{
    public int Id { get; set; }
    public string Address { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime AssignedAt { get; set; }
}

