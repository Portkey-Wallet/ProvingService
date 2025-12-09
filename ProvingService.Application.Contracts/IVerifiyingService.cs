using System.Threading.Tasks;

namespace ProvingService.Application.Contracts;

public interface IVerifyingService
{
    Task<bool> VerifyAsync(VerifyInput request);
    string GetVerifyingKey();
    string GetZkeyMd5();
}