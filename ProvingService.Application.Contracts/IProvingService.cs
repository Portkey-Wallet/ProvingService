using System.Threading.Tasks;

namespace ProvingService.Application.Contracts;

public interface IProvingService
{
    Task<(string, string)> ProveAsync(ProveInput request);
}