namespace ProvingService.Controllers;

public class VerifyRequest
{
    public string IdentifierHash { get; set; }
    public string Salt { get; set; }
    public string Nonce { get; set; }
    public string Kid { get; set; }
    public string Proof { get; set; }
}