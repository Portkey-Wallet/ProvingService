using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AElf;
using AElf.Types;
using ProvingService.Application.Contracts;
using ProvingService.Application.Internal;

namespace ProvingService.Application.Sha256;

public class Sha256IdentifierHashService : IIdentifierHashService
{
    public string GenerateIdentifierHash(string subject, byte[] salt)
    {
        var identifierHash = GetHash(Encoding.UTF8.GetBytes(subject), salt);
        return identifierHash.Value.ToHex();
    }

    public List<string> ToPublicInput(string identifierHash)
    {
        return identifierHash.HexStringToByteArray().Select(b => b.ToString()).ToList();
    }


    private static Hash GetHash(byte[] identifier, byte[] salt)
    {
        const int maxIdentifierLength = 256;
        const int maxSaltLength = 16;

        if (identifier.Length > maxIdentifierLength)
        {
            throw new Exception("Identifier is too long");
        }

        if (salt.Length != maxSaltLength)
        {
            throw new Exception($"Salt has to be {maxSaltLength} bytes.");
        }

        var hash = AElf.HashHelper.ComputeFrom(identifier);
        return AElf.HashHelper.ComputeFrom(hash.Concat(salt).ToArray());
    }
}