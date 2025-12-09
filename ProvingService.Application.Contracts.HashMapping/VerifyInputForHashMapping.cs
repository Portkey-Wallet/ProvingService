namespace ProvingService.Application.Contracts.HashMapping;

public class VerifyInputForHashMapping
{
    public string PoseidonIdHash { get; set; } = null!;
    public string Sha256IdHash { get; set; } = null!;
    public string Proof { get; set; } = null!;
}