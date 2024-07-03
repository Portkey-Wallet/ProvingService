namespace ProvingService.Application.Contracts
{
    public class VerifyInput
    {
        public string IdentifierHash { get; set; } = null!;
        public string Salt { get; set; } = null!;
        public string Nonce { get; set; } = null!;
        public string Kid { get; set; } = null!;
        public string Proof { get; set; } = null!;
    }
}