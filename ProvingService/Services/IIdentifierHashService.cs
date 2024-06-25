using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AElf;
using ProvingService.Helpers;

namespace ProvingService.Services;

public interface IIdentifierHashService
{
    string GenerateIdentifierHash(string subject, byte[] salt);
    List<string> ToPublicInput(string identifierHash);
}

public class Sha256IdentifierHashService : IIdentifierHashService
{
    public string GenerateIdentifierHash(string subject, byte[] salt)
    {
        var identifierHash = Helpers.HashHelper.GetHash(Encoding.UTF8.GetBytes(subject), salt);
        return identifierHash.Value.ToHex();
    }

    public List<string> ToPublicInput(string identifierHash)
    {
        return identifierHash.HexStringToByteArray().Select(b => b.ToString()).ToList();
    }
}

public class PoseidonIdentifierHashService : IIdentifierHashService
{
    public string GenerateIdentifierHash(string subject, byte[] salt)
    {
        return "Not implemented";
    }

    public List<string> ToPublicInput(string identifierHash)
    {
        return [identifierHash];
    }
}