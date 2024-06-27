using System.Collections.Generic;
using System.Linq;
using ProvingService.Domain.Common;
using ProvingService.Domain.Common.Extensions;

namespace ProvingService.Domain.HashMapping;

public static class PublicInputPreparer
{
    private static bool IsValidSha256String(string str)
    {
        var pattern = @"^(0[xX])?[0-9a-fA-F]{64}$";
        return System.Text.RegularExpressions.Regex.IsMatch(str, pattern);
    }

    public static List<string> Prepare(string sha256IdHash, string poseidonIdHash)
    {
        if (poseidonIdHash.Length > 77 || !poseidonIdHash.All(char.IsDigit))
        {
            throw new InvalidValueException("PoseidonIdHash should be a number of up to 77 digits.");
        }

        if (!IsValidSha256String(sha256IdHash))
        {
            throw new InvalidValueException("Sha256IdHash should be valid hex representation.");
        }

        var sha256IdHashArray = sha256IdHash.HexStringToByteArray().Select(b => b.ToString()).ToList();

        return new List<List<string>>
        {
            new List<string>() { poseidonIdHash }, sha256IdHashArray
        }.SelectMany(n => n).ToList();
    }
}