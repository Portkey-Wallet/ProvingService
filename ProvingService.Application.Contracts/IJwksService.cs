using System.Threading.Tasks;

namespace ProvingService.Application.Contracts;

public interface IJwksService
{
    Task<string> GetKeyAsync(string kid);
}