namespace ProvingService.Controllers;

public class ProveResponse
{
    public string Proof { get; set; }
    public string IdentifierHash { get; set; }
    public string CircuitId { get; set; }
}