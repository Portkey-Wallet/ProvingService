using System.Collections.Generic;
using System.Linq;
using System.Text;
using AElf;
using AElf.Types;
using ProvingService.Application.Contracts;
using ProvingService.Domain.Common;
using ProvingService.Domain.Common.Extensions;
using ProvingService.Domain.HashMapping;

namespace ProvingService.Domain.Sha256Hash;

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


    private static Hash GetHash(byte[] subject, byte[] salt)
    {
        if (subject.Length > CircuitParameters.MaxSubLength)
        {
            throw new InputExceedingMaxLengthException(
                $"Input subject exceeding max length of {CircuitParameters.MaxSubLength}.");
        }

        if (salt.Length > CircuitParameters.SaltLength)
        {
            throw new InputExceedingMaxLengthException(
                $"Input salt exceeding max length of {CircuitParameters.SaltLength}.");
        }


        var hash = AElf.HashHelper.ComputeFrom(subject);
        return AElf.HashHelper.ComputeFrom(hash.Concat(salt).ToArray());
    }
}