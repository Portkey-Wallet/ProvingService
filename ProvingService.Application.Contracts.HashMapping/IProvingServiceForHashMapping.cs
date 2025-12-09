using System.Threading.Tasks;

namespace ProvingService.Application.Contracts.HashMapping;

public interface IProvingServiceForHashMapping
{
    Task<ProveOutputForHashMapping> ProveAsync(ProveInputForHashMapping input);
}