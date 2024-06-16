using System;
using System.Linq;
using AElf.Types;

namespace ProvingService.Helpers;

public class HashHelper
{
    public static Hash GetHash(byte[] identifier, byte[] salt)
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