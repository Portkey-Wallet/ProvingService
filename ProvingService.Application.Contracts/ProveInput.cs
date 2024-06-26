namespace ProvingService.Application.Contracts;

public class ProveInput
{
    public string Jwt { get; set; }
    public string Salt { get; set; }
}