using System.Threading.Tasks;

namespace ProvingService.Application.Contracts.HashMapping;

public interface IVerifyingServiceForHashMapping
{
    Task<bool> VerifyAsync(VerifyInputForHashMapping input);
    string GetVerifyingKey();
}