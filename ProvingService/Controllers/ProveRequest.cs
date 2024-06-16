namespace ProvingService.Controllers;

public class ProveRequest
{
    public string Jwt { get; set; }
    public string Salt { get; set; }
}