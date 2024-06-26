using System.Collections.Generic;

namespace ProvingService.Application.Contracts;

public interface IIdentifierHashService
{
    string GenerateIdentifierHash(string subject, byte[] salt);
    List<string> ToPublicInput(string identifierHash);
}